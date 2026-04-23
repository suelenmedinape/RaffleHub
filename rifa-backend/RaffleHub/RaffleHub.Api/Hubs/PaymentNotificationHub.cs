using Microsoft.AspNetCore.SignalR;

namespace RaffleHub.Api.Hubs;

public class PaymentNotificationHub : Hub
{
    // O cliente frontend se conecta e entra em um grupo com o mesmo nome do TransactionId (ou ID da reserva)
    public async Task JoinPaymentGroup(string transactionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, transactionId);
    }

    public async Task LeavePaymentGroup(string transactionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, transactionId);
    }
}
