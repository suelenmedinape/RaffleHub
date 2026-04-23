using FluentResults;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Hubs;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Features.Payments.ConfirmPayment;

public class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, Result>
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<ConfirmPaymentHandler> _logger;
    private readonly IHubContext<PaymentNotificationHub> _hubContext;

    public ConfirmPaymentHandler(
        IBookingRepository repository,
        ILogger<ConfirmPaymentHandler> logger,
        IHubContext<PaymentNotificationHub> hubContext)
    {
        _repository = repository;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<Result> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        await _repository.BeginTransactionAsync();
        try
        {
            // Use pessimistic locking (FOR UPDATE) to prevent race conditions
            var booking = await _repository.GetByTransactionIdLockedAsync(request.TransactionId);

            if (booking == null)
            {
                await _repository.RollbackTransactionAsync();
                return Result.Fail("Reserva não encontrada para essa transação");
            }

            _logger.LogInformation("Webhook: Processando reserva {BookingId}. Status Atual: {Status}", booking.Id, booking.Status);

            if (booking.Status != BookingStatus.PAID)
            {
                booking.Status = BookingStatus.PAID;
                booking.PaidAt = DateTime.UtcNow;
                _repository.Update(booking);
                await _repository.SaveChangesAsync();
            }
            
            await _repository.CommitTransactionAsync();

            _logger.LogInformation("🚀 Disparando SignalR PaymentConfirmed para Grupo: {GroupId}", booking.Id);

            // Emitir evento em tempo real via SignalR para o frontend
            await _hubContext.Clients.Group(booking.Id.ToString()).SendAsync("PaymentConfirmed", new
            {
                numbers = booking.Tickets.Select(t => t.TicketNumber.ToString()).ToList(),
                raffleName = booking.Raffle.RaffleName
            }, cancellationToken);
            _logger.LogInformation("SignalR Evento PaymentConfirmed enviado com sucesso!");

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await _repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Erro ao confirmar pagamento para a transação {TransactionId}", request.TransactionId);
            return Result.Fail("Erro interno ao confirmar pagamento");
        }
    }
}
