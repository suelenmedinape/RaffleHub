using System.Security.Claims;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.DTOs.Participant;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;
using Supabase.Gotrue;

namespace RaffleHub.Api.Services;

public class ParticipantService
{
    private readonly IParticipantRepository _repository;
    private readonly IRaffleRepository _raffleRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ParticipantService(IParticipantRepository repository, IRaffleRepository raffleRepository,
        ITicketRepository ticketRepository, IBookingRepository bookingRepository, IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _raffleRepository = raffleRepository;
        _ticketRepository = ticketRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<IEnumerable<ListAllParticipantsDto>>> ListByRaffle(Guid raffleId, int page = 1, int pageSize = 50, string? searchTerm = null)
    {
        var raffle = await _raffleRepository.GetByIdAsync(raffleId);
        if (raffle == null)
            return Result.Fail("Rifa não encontrada.");

        var query = _repository.GetQueryable()
            .Include(p => p.Tickets)
            .Where(p => p.Tickets.Any(t => t.RaffleId == raffleId));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();
            if (int.TryParse(searchTerm, out int ticketNumber))
            {
                query = query.Where(p => p.Tickets.Any(t => t.RaffleId == raffleId && t.TicketNumber == ticketNumber));
            }
            else
            {
                var lowerSearch = $"%{searchTerm.ToLower()}%";
                query = query.Where(p => EF.Functions.Like(p.ParticipantName.ToLower(), lowerSearch) || 
                                         EF.Functions.Like(p.Phone.ToLower(), lowerSearch));
            }
        }

        var participants = await query
            .OrderBy(p => p.ParticipantName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = participants
            .Select(p => _mapper.Map<ListAllParticipantsDto>(p))
            .ToList();

        return Result.Ok<IEnumerable<ListAllParticipantsDto>>(result);
    }
    
    public async Task<Result<ParticipantDetailDto>> GetById(Guid id)
    {
        var result = await _repository.GetByIdAsync(id);
        if (result == null)
            return Result.Fail("Participante não encontrada.");

        var dto = _mapper.Map<ParticipantDetailDto>(result);

        return Result.Ok(dto);
    }

    public async Task<Result<ParticipantPurchaseResponseDto>> CreateParticipant(CreateParticipantDto dto)
    {
        if (dto.TicketNumbers == null || !dto.TicketNumbers.Any())
            return Result.Fail("Nenhum número de rifa foi selecionado.");

        if (dto.TicketNumbers.Distinct().Count() != dto.TicketNumbers.Count)
            return Result.Fail("Existem números duplicados na sua solicitação.");

        var currentUser = _httpContextAccessor.HttpContext?.User;
        var isAdminOrManager = currentUser != null && (currentUser.IsInRole("OPERATOR") || currentUser.IsInRole("ADMIN"));
        
        var raffleExist = await _raffleRepository.GetByIdAsync(dto.RaffleId);
        if (raffleExist == null)
            return Result.Fail("Rifa não encontrada.");

        if (raffleExist.Status != RaffleStatus.ACTIVE)
            return Result.Fail("Não é mais permitido participar dessa rifa.");

        var invalidTicket = dto.TicketNumbers.FirstOrDefault(n => n < 1 || n > raffleExist.TotalTickets);
        if (invalidTicket != 0)
            return Result.Fail($"O número {invalidTicket} é inválido para esta rifa.");

        var soldTickets = await _ticketRepository.GetQueryable()
            .Where(t => t.RaffleId == dto.RaffleId && dto.TicketNumbers.Contains(t.TicketNumber))
            .Select(t => t.TicketNumber)
            .ToListAsync();

        if (soldTickets.Any())
            return Result.Fail($"Os seguintes números já estão reservados ou foram vendidos: {string.Join(", ", soldTickets)}");

        await _bookingRepository.BeginTransactionAsync();
        try
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var participant = await _repository.GetQueryable().FirstOrDefaultAsync(p => p.Phone == dto.Phone);

            if (participant == null)
            {
                participant = _mapper.Map<Participant>(dto);
                participant.UserId = currentUserId;
                _repository.Add(participant);
                await _repository.SaveChangesAsync();
            }
            else
            {
                if (participant.UserId == null && currentUserId != null)
                {
                    participant.UserId = currentUserId;
                    _repository.Update(participant);
                    await _repository.SaveChangesAsync();
                }

                var hasPendingBookings = await _bookingRepository.GetQueryable()
                    .AnyAsync(b => b.ParticipantId == participant.Id && b.Status == BookingStatus.PENDING);

                if (hasPendingBookings)
                {
                    await _bookingRepository.RollbackTransactionAsync();
                    return Result.Fail("É preciso pagar pela compra anterior para poder realizar uma nova compra");
                }
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                ParticipantId = participant.Id,
                Participant = participant,
                RaffleId = raffleExist.Id,
                Raffle = raffleExist,
                TotalAmount = dto.TicketNumbers.Count * raffleExist.TicketPrice,
                Status = isAdminOrManager ? BookingStatus.PAID : BookingStatus.PENDING,
                Tickets = dto.TicketNumbers.Select(ticketNumber => new Ticket
                {
                    Id = Guid.NewGuid(),
                    TicketNumber = ticketNumber,
                    RaffleId = dto.RaffleId,
                    Raffle = raffleExist,
                    ParticipantId = participant.Id,
                    Participant = participant
                }).ToList()
            };

            _bookingRepository.Add(booking);
            await _bookingRepository.SaveChangesAsync();
            await _bookingRepository.CommitTransactionAsync();

            return Result.Ok(_mapper.Map<ParticipantPurchaseResponseDto>(booking));
        }
        catch (Exception)
        {
            await _bookingRepository.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Result> DeleteParticipant(Guid participantId, Guid raffleId)
    {
        var raffle = await _raffleRepository.GetByIdAsync(raffleId);
        if (raffle == null)
            return Result.Fail("Rifa não encontrada.");

        var participant = await _repository.GetByIdAsync(participantId);
        if (participant == null)
            return Result.Fail("Participante não encontrada.");
        var tickets = await _ticketRepository.GetQueryable()
            .Where(t => t.ParticipantId == participantId)
            .ToListAsync();

        foreach (var ticket in tickets)
        {
            _ticketRepository.Delete(ticket);
        }
        
        _repository.Delete(participant);
        await _repository.SaveChangesAsync();

        if (raffle.Status == RaffleStatus.COMPLETED)
        {
            raffle.Status = RaffleStatus.ACTIVE;
            _raffleRepository.Update(raffle);
            await _raffleRepository.SaveChangesAsync();
        }

        return Result.Ok();
    }
}