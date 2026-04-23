using FluentResults;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Hubs;
using RaffleHub.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RaffleHub.Api.Features.Payments.ConfirmManualPayment;

public class ConfirmManualPaymentHandler : IRequestHandler<ConfirmManualPaymentCommand, Result>
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<ConfirmManualPaymentHandler> _logger;
    private readonly IHubContext<PaymentNotificationHub> _hubContext;

    public ConfirmManualPaymentHandler(
        IBookingRepository repository,
        ILogger<ConfirmManualPaymentHandler> logger,
        IHubContext<PaymentNotificationHub> hubContext)
    {
        _repository = repository;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<Result> Handle(ConfirmManualPaymentCommand request, CancellationToken cancellationToken)
    {
        await _repository.BeginTransactionAsync();
        try
        {
            // Fetch the booking with pessimistic locking if needed, though manual is less concurrent
            var booking = await _repository.GetQueryable()
                .Include(b => b.Raffle)
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
            {
                await _repository.RollbackTransactionAsync();
                return Result.Fail("Reserva não encontrada.");
            }

            if (booking.Status == BookingStatus.PAID)
            {
                await _repository.RollbackTransactionAsync();
                return Result.Fail("Esta reserva já está paga.");
            }

            _logger.LogInformation("Confirmando pagamento manual para Reserva {BookingId}. Status Anterior: {Status}", booking.Id, booking.Status);

            booking.Status = BookingStatus.PAID;
            booking.PaidAt = DateTime.UtcNow;
            
            _repository.Update(booking);
            await _repository.SaveChangesAsync();
            await _repository.CommitTransactionAsync();

            _logger.LogInformation("🚀 Disparando SignalR PaymentConfirmed (Manual) para Grupo: {GroupId}", booking.Id);

            // Emitir evento em tempo real via SignalR para o frontend
            await _hubContext.Clients.Group(booking.Id.ToString()).SendAsync("PaymentConfirmed", new
            {
                numbers = booking.Tickets.Select(t => t.TicketNumber.ToString()).ToList(),
                raffleName = booking.Raffle.RaffleName
            }, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await _repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Erro ao confirmar pagamento manual para a reserva {BookingId}", request.BookingId);
            return Result.Fail("Erro interno ao confirmar pagamento manual.");
        }
    }
}
