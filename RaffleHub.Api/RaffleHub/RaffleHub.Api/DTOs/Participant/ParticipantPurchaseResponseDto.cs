using RaffleHub.Api.Enums;

namespace RaffleHub.Api.DTOs.Participant;

public class ParticipantPurchaseResponseDto
{
    public Guid BookingId { get; set; }
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string RaffleName { get; set; } = string.Empty;
    
    public List<int> TicketNumbers { get; set; } = new();
    public decimal TotalAmount { get; set; }
    
    public BookingStatus Status { get; set; } 
}