using RaffleHub.Api.DTOs.Ticket;
using RaffleHub.Api.Enums;

namespace RaffleHub.Api.DTOs.Booking;

public class ListBookingPendingDto
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid RaffleId { get; set; }
    public string RaffleName { get; set; } = string.Empty;
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;

    public string? PixQrCodeUrl { get; set; }
    public string? PixCopyPaste { get; set; }

    // Use um DTO aqui ao invés de Entities.Ticket
    public ICollection<ListTicketsByParticipant> Tickets { get; set; } = new List<ListTicketsByParticipant>();
}
