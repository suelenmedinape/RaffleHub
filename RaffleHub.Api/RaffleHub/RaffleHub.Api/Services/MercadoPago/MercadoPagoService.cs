using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;

namespace RaffleHub.Api.Services.MercadoPago;

public class MercadoPagoService : IMercadoPagoService
{
    private const string BaseUrl = "https://api.mercadopago.com";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MercadoPagoService> _logger;

    public MercadoPagoService(HttpClient httpClient, IConfiguration configuration, ILogger<MercadoPagoService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<MercadoPagoPixResponseDto>> CreatePixTransactionAsync(
        decimal amount, string payerName, string payerDocument, string externalReference)
    {
        try
        {
            var accessToken = _configuration["MercadoPago:AccessToken"];
            var useFakeApi = _configuration.GetValue<bool>("MercadoPago:UseFakeApi");

            if (useFakeApi || string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("MercadoPago:AccessToken not configured or UseFakeApi is true. Returning mock response.");
                return Result.Ok(new MercadoPagoPixResponseDto
                {
                    TransactionId = "MOCK_" + Guid.NewGuid().ToString(),
                    PixQrCodeUrl = "https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=00020126850014br.gov.bcb.pix",
                    PixCopyPaste = "00020126850014br.gov.bcb.pix2563pix.example.com/qr/mock5204000053039865802BR5912RaffleHub6007GOIANIA6304ABCD"
                });
            }

            // Sanitize document: remove non-digits
            var cpf = new string(payerDocument.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
            {
                _logger.LogWarning("MercadoPago: CPF fornecido tem {Count} dígitos. O Mercado Pago exige 11 dígitos para CPF.", cpf.Length);
                return Result.Fail("O CPF deve conter exatamente 11 números para pagamentos PIX.");
            }

            var requestBody = new
            {
                transaction_amount = amount,
                description = "Pagamento de Rifa",
                payment_method_id = "pix",
                external_reference = externalReference,
                payer = new
                {
                    email = $"pagador-{cpf}@rifahub.com", // MP requer e-mail; usamos placeholder único
                    first_name = payerName.Split(' ').FirstOrDefault() ?? payerName,
                    last_name = payerName.Split(' ').Skip(1).LastOrDefault() ?? "",
                    identification = new
                    {
                        type = "CPF",
                        number = cpf
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", externalReference);

            var response = await _httpClient.PostAsync($"{BaseUrl}/v1/payments", content);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MercadoPago API Error: {StatusCode} {Body}", response.StatusCode, jsonResponse);
                return Result.Fail($"Erro no Mercado Pago: {response.ReasonPhrase}");
            }

            var mpResult = JsonSerializer.Deserialize<MercadoPagoPaymentResponse>(jsonResponse);

            if (mpResult == null)
                return Result.Fail("Erro ao processar resposta do Mercado Pago.");

            var qrCodeBase64 = mpResult.PointOfInteraction?.TransactionData?.QrCodeBase64;
            var qrCodeUrl = !string.IsNullOrEmpty(qrCodeBase64)
                ? $"data:image/png;base64,{qrCodeBase64}"
                : $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(mpResult.PointOfInteraction?.TransactionData?.QrCode ?? "")}";

            return Result.Ok(new MercadoPagoPixResponseDto
            {
                TransactionId = mpResult.Id.ToString(),
                PixQrCodeUrl = qrCodeUrl,
                PixCopyPaste = mpResult.PointOfInteraction?.TransactionData?.QrCode ?? ""
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar transação PIX no Mercado Pago.");
            return Result.Fail("Erro interno ao processar pagamento.");
        }
    }

    public async Task<Result<MercadoPagoStatusResponseDto>> CheckTransactionStatusAsync(string transactionId)
    {
        try
        {
            var accessToken = _configuration["MercadoPago:AccessToken"];
            if (string.IsNullOrEmpty(accessToken) || transactionId.StartsWith("MOCK_"))
            {
                return Result.Ok(new MercadoPagoStatusResponseDto { TransactionId = transactionId, Status = "PAID" });
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{BaseUrl}/v1/payments/{transactionId}");
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MercadoPago Status Check API Error: {StatusCode} {Body}", response.StatusCode, jsonResponse);
                return Result.Fail("Erro ao checar status no Mercado Pago.");
            }

            var mpResult = JsonSerializer.Deserialize<MercadoPagoPaymentResponse>(jsonResponse);
            
            // Mapeamento de status: approved -> PAID, outros -> PENDING
            // Para simplicidade no Job, o importante é saber se está pago ("COMPLETO" no MisticPay era "approved")
            var status = mpResult?.Status == "approved" ? "PAID" : "PENDING";

            return Result.Ok(new MercadoPagoStatusResponseDto
            {
                TransactionId = transactionId,
                Status = status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao checar status no Mercado Pago.");
            return Result.Fail("Erro interno ao checar status.");
        }
    }

    /// <summary>
    /// Valida a assinatura do webhook enviada pelo Mercado Pago.
    /// Documentação: https://www.mercadopago.com.br/developers/pt/docs/your-integrations/notifications/webhooks
    /// </summary>
    public static bool ValidateSignature(string xSignature, string xRequestId, string dataId, string secret)
    {
        // Formato: ts=<timestamp>,v1=<hash>
        var parts = xSignature.Split(',');
        var ts = parts.FirstOrDefault(p => p.StartsWith("ts="))?.Substring(3);
        var v1 = parts.FirstOrDefault(p => p.StartsWith("v1="))?.Substring(3);

        if (string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(v1))
            return false;

        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts};";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest));
        var generatedHash = BitConverter.ToString(hash).Replace("-", "").ToLower();

        return generatedHash == v1;
    }

    // ── Modelos de resposta internos ──────────────────────────────────────────

    private class MercadoPagoPaymentResponse
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("external_reference")] public string ExternalReference { get; set; } = "";
        [JsonPropertyName("point_of_interaction")] public MpPointOfInteraction? PointOfInteraction { get; set; }
    }

    private class MpPointOfInteraction
    {
        [JsonPropertyName("transaction_data")] public MpTransactionData? TransactionData { get; set; }
    }

    private class MpTransactionData
    {
        [JsonPropertyName("qr_code")] public string? QrCode { get; set; }
        [JsonPropertyName("qr_code_base64")] public string? QrCodeBase64 { get; set; }
    }
}
