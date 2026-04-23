using System.Diagnostics.CodeAnalysis;
using RaffleHub.Api.Enums;

namespace RaffleHub.Api.DTOs.Raffle;

public class UpdateStatusDto
{
    [NotNull]
    public RaffleStatus Status { get; set; }
}