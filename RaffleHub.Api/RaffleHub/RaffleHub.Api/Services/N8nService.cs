using System.Text;
using System.Text.Json;
using RaffleHub.Api.Services.Interface;

namespace RaffleHub.Api.Services;

public class N8nService : IN8nService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<N8nService> _logger;

    public N8nService(HttpClient httpClient, IConfiguration configuration, ILogger<N8nService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWhatsAppNotificationAsync(string phone, string participantName, List<int> tickets, string raffleName)
    {
        var webhookUrl = _configuration["N8n:WebhookUrl"];
        if (string.IsNullOrEmpty(webhookUrl))
        {
            _logger.LogWarning("n8n Webhook URL is not configured. Skipping WhatsApp notification.");
            return;
        }

        var payload = new
        {
            phone = phone,
            name = participantName,
            raffleName = raffleName,
            tickets = tickets,
            message = $"Obrigado por comprar nossa rifa {raffleName}, os numeros adquiridos foram [{string.Join(", ", tickets)}]. Boa Sorte!"
        };

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send notification to n8n. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorBody);
            }
            else
            {
                _logger.LogInformation("Successfully sent WhatsApp notification for {ParticipantName} to n8n", participantName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending notification to n8n for {ParticipantName}", participantName);
        }
    }
}
