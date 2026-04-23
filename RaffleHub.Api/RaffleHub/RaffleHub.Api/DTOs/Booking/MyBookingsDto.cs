namespace RaffleHub.Api.DTOs.Booking;

public class MyBookingsDto
{
    public Guid Id { get; set; }
    public Guid RaffleId { get; set; }
    public Guid ParticipantId { get; set; }
    public string RaffleName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IList<int> TicketNumbers { get; set; } = new List<int>();
}
