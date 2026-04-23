using FluentResults;

namespace RaffleHub.Api.Services.MercadoPago;

public interface IMercadoPagoService
{
    Task<Result<MercadoPagoPixResponseDto>> CreatePixTransactionAsync(decimal amount, string payerName, string payerDocument, string externalReference);
    Task<Result<MercadoPagoStatusResponseDto>> CheckTransactionStatusAsync(string transactionId);
}

public class MercadoPagoStatusResponseDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // approved, pending, cancelled, etc.
}

public class MercadoPagoPixResponseDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string PixQrCodeUrl { get; set; } = string.Empty;
    public string PixCopyPaste { get; set; } = string.Empty;
}
