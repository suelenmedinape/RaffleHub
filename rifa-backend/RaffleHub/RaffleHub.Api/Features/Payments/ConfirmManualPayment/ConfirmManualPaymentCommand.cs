using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;
using MediatR;

namespace RaffleHub.Api.Features.Payments.ConfirmManualPayment;

public record ConfirmManualPaymentCommand(Guid BookingId) : IRequest<FluentResults.Result>;
