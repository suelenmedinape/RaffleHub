using RaffleHub.Api.Enums;

namespace RaffleHub.Api.DTOs.Raffle;

public class ListAllRaffleDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    public required string RaffleName { get; set; }
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
    public DateTime DrawDate { get; set; }
    public RaffleStatus Status { get; set; }
    public int SoldTicketsCount { get; set; }
}