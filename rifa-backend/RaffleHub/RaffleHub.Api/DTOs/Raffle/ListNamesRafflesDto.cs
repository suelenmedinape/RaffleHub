namespace RaffleHub.Api.DTOs.Raffle;

public class ListNamesRafflesDto
{
    public Guid Id { get; set; }
    public required string RaffleName { get; set; }
}