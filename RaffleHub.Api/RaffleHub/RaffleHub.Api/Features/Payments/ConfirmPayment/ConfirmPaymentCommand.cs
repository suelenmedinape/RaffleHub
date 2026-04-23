using FluentResults;
using MediatR;

namespace RaffleHub.Api.Features.Payments.ConfirmPayment;

public record ConfirmPaymentCommand(string TransactionId) : IRequest<Result>;
