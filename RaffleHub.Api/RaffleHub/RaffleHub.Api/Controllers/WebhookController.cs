using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.DTOs.Webhook;
using RaffleHub.Api.Services;
using RaffleHub.Api.Services.MercadoPago;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WebhookController : ControllerBase
{
    private readonly BookingService _bookingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(BookingService bookingService, IConfiguration configuration, ILogger<WebhookController> logger)
    {
        _bookingService = bookingService;
        _configuration = configuration;
        _logger = logger;
    }



    [HttpPost("mercadopago")]
    public async Task<IActionResult> MercadoPagoWebhook([FromBody] MercadoPagoWebhookDto webhook)
    {
        // Audit log: registra toda tentativa antes de processar
        _logger.LogInformation(
            "Tentativa de webhook MercadoPago: Id={Id}, Action={Action}, Type={Type}, DataId={DataId}",
            webhook.Id, webhook.Action, webhook.Type, webhook.Data?.Id);

        // Validar assinatura HMAC-SHA256
        /* var secret = _configuration["MercadoPago:WebhookSecret"];
        if (!string.IsNullOrEmpty(secret))
        {
            var xSignature = Request.Headers["x-signature"].FirstOrDefault() ?? "";
            var xRequestId = Request.Headers["x-request-id"].FirstOrDefault() ?? "";
            var dataId = webhook.Data?.Id ?? "";

            if (!MercadoPagoService.ValidateSignature(xSignature, xRequestId, dataId, secret))
            {
                _logger.LogWarning("Webhook MercadoPago rejeitado: assinatura inválida.");
                return Unauthorized("Assinatura inválida");
            }
        } */

        // O MP envia notificações para vários eventos: só nos interessa "payment" com "payment.updated"
        if (webhook.Type != "payment" || webhook.Data == null || string.IsNullOrEmpty(webhook.Data.Id))
        {
            _logger.LogInformation("Evento ignorado: Type={Type}", webhook.Type);
            return Ok(new { message = "Evento ignorado" });
        }

        var transactionId = webhook.Data.Id;

        _logger.LogInformation("Iniciando confirmação de pagamento para TransactionId={TransactionId}", transactionId);
        var result = await _bookingService.ConfirmPaymentByTransactionIdAsync(transactionId);

        if (result.IsFailed)
        {
            _logger.LogWarning("Falha ao confirmar pagamento para TransactionId={TransactionId}: {Errors}",
                transactionId, string.Join(", ", result.Errors));
            // Retornamos 200 mesmo em falha para evitar reenvios desnecessários do MP
            return Ok(new { message = "Pagamento não encontrado ou já processado" });
        }

        _logger.LogInformation("Webhook MercadoPago processado com sucesso para TransactionId={TransactionId}", transactionId);
        return Ok(new { message = "Pagamento confirmado com sucesso" });
    }
}
