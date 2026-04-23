namespace RaffleHub.Api.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public int TicketNumber { get; set; }
    
    public Guid RaffleId { get; set; }
    public virtual Raffle Raffle { get; set; }
    
    public Guid? ParticipantId { get; set; }
    public virtual Participant? Participant { get; set; }
    
    public Guid? BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
}