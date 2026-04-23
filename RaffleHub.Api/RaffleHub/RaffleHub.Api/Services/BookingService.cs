using System.Security.Claims;
using AutoMapper;
using FluentResults;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.DTOs.Booking;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services.MercadoPago;
using RaffleHub.Api.Services.Interface;
using RaffleHub.Api.Utils.Job;
using Microsoft.AspNetCore.SignalR;
using RaffleHub.Api.Hubs;
using MediatR;

namespace RaffleHub.Api.Services;

public class BookingService
{
    private readonly IBookingRepository _repository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly ILogger<BookingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHubContext<PaymentNotificationHub> _hubContext;
    private readonly IMediator _mediator;

    public BookingService(
        IBookingRepository repository, 
        IParticipantRepository participantRepository, 
        IMercadoPagoService mercadoPagoService,
        IMapper mapper,
        IBackgroundJobClient backgroundJobs,
        ILogger<BookingService> logger,
        IHttpContextAccessor httpContextAccessor,
        IHubContext<PaymentNotificationHub> hubContext,
        IMediator mediator)
    {
        _repository = repository;
        _participantRepository = participantRepository;
        _mercadoPagoService = mercadoPagoService;
        _mapper = mapper;
        _backgroundJobs = backgroundJobs;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task<Result<IEnumerable<MyBookingsDto>>> GetMyBookingsAsync()
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Result.Fail("Usuário não autenticado");

        // Lazy Link: se o usuário tiver um telefone no Token, vincula participantes órfãos
        var userPhone = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.MobilePhone);
        if (!string.IsNullOrEmpty(userPhone))
        {
            var sanitizedPhone = new string(userPhone.Where(char.IsDigit).ToArray());
            var orphanParticipants = await _participantRepository.GetQueryable()
                .Where(p => p.Phone == sanitizedPhone && p.UserId == null)
                .ToListAsync();

            if (orphanParticipants.Any())
            {
                foreach (var p in orphanParticipants)
                {
                    p.UserId = currentUserId;
                    _participantRepository.Update(p);
                }
                await _participantRepository.SaveChangesAsync();
            }
        }

        var bookings = await _repository.GetQueryable()
            .Include(b => b.Raffle)
            .Include(b => b.Tickets)
            .Include(b => b.Participant)
            .Where(b => b.Participant != null && b.Participant.UserId == currentUserId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var dtos = bookings.Select(b => new MyBookingsDto
        {
            Id = b.Id,
            RaffleId = b.RaffleId,
            ParticipantId = b.ParticipantId,
            RaffleName = b.Raffle?.RaffleName ?? "N/A",
            TotalAmount = b.TotalAmount,
            Status = b.Status.ToString(),
            CreatedAt = b.CreatedAt,
            TicketNumbers = b.Tickets.Select(t => t.TicketNumber).ToList()
        });

        return Result.Ok<IEnumerable<MyBookingsDto>>(dtos);
    }

    public async Task<Result<ListBookingPendingDto>> BookingPending(Guid participantId)
    {
        var participant = await _participantRepository.GetByIdAsync(participantId);
        if (participant == null)
            return Result.Fail("Esse participante não existe");
    
        var pendingBooking = await _repository.GetQueryable()
            .Include(b => b.Tickets)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(b => b.ParticipantId == participantId && 
                (b.Status == BookingStatus.PENDING || b.Status == BookingStatus.PAID));

        if (pendingBooking == null)
            return Result.Fail("Nenhuma reserva encontrada para esse participante.");

        var dto = _mapper.Map<ListBookingPendingDto>(pendingBooking);
    
        return Result.Ok(dto);
    }

    public async Task<Result<ListBookingPendingDto>> GeneratePixPaymentAsync(Guid participantId)
    {
        var participant = await _participantRepository.GetByIdAsync(participantId);
        if (participant == null)
            return Result.Fail("Participante não encontrado");

        var booking = await _repository.GetQueryable()
            .Include(b => b.Tickets)
            .Include(b => b.Raffle)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(b => b.ParticipantId == participantId);

        if (booking == null)
            return Result.Fail("Nenhuma reserva encontrada");

        if (booking.Status == BookingStatus.EXPIRED || booking.Status == BookingStatus.CANCELLED)
            return Result.Fail("Sua reserva já expirou. Por favor, tente novamente na tela da rifa.");

        if (booking.Status == BookingStatus.PAID)
            return Result.Fail("O pagamento desta reserva já foi confirmado!");

        // Se o PIX já foi gerado na reserva atual, apenas o retorne em vez de criar um novo.
        if (!string.IsNullOrEmpty(booking.PixQrCodeUrl) && !string.IsNullOrEmpty(booking.PixCopyPaste))
        {
            return Result.Ok(_mapper.Map<ListBookingPendingDto>(booking));
        }

        var pixResult = await _mercadoPagoService.CreatePixTransactionAsync(
            booking.TotalAmount, 
            participant.ParticipantName, 
            participant.Cpf,
            booking.Id.ToString()
        );

        if (pixResult.IsFailed)
            return Result.Fail(pixResult.Errors);

        booking.TransactionId = pixResult.Value.TransactionId;
        _logger.LogInformation("Pix Gerado: Booking={BookingId}, TransactionId={TransactionId}", booking.Id, booking.TransactionId);
        booking.PixQrCodeUrl = pixResult.Value.PixQrCodeUrl;
        booking.PixCopyPaste = pixResult.Value.PixCopyPaste;
        
        _repository.Update(booking);
        await _repository.SaveChangesAsync();

        if (booking.TransactionId.StartsWith("MOCK_"))
        {
            _backgroundJobs.Schedule<BookingJob>(
                job => job.ExpireBookingAsync(booking.Id),
                TimeSpan.FromSeconds(10)
            );
        }
        else
        {
            _backgroundJobs.Schedule<BookingJob>(
                job => job.ExpireBookingAsync(booking.Id),
                TimeSpan.FromMinutes(30)
            );
        }

        return Result.Ok(_mapper.Map<ListBookingPendingDto>(booking));
    }

    public async Task<Result> ConfirmPaymentByTransactionIdAsync(string transactionId)
    {
        return await _mediator.Send(new Features.Payments.ConfirmPayment.ConfirmPaymentCommand(transactionId));
    }

    public async Task<Result> ConfirmManualPaymentAsync(Guid bookingId)
    {
        return await _mediator.Send(new Features.Payments.ConfirmManualPayment.ConfirmManualPaymentCommand(bookingId));
    }
}
