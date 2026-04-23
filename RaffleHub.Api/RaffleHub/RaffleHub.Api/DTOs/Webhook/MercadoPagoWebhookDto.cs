using System.Text.Json.Serialization;

namespace RaffleHub.Api.DTOs.Webhook;

/// <summary>
/// Payload enviado pelo Mercado Pago nas notificações de webhook.
/// Documentação: https://www.mercadopago.com.br/developers/pt/docs/your-integrations/notifications/webhooks
/// </summary>
public class MercadoPagoWebhookDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("action")] public string Action { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("data")] public MercadoPagoWebhookData? Data { get; set; }
}

public class MercadoPagoWebhookData
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
}
