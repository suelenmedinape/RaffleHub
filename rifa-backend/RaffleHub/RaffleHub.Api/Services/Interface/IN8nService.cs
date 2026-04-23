namespace RaffleHub.Api.Services.Interface;

public interface IN8nService
{
    Task SendWhatsAppNotificationAsync(string phone, string participantName, List<int> tickets, string raffleName);
}
