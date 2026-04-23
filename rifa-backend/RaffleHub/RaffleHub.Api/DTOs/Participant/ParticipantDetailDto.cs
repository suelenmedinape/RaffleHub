using RaffleHub.Api.DTOs.Ticket;

namespace RaffleHub.Api.DTOs.Participant;

public class ParticipantDetailDto
{
    public Guid Id { get; set; }
    public string ParticipantName { get; set; }
    public string Phone { get; set; }
    
    //public Guid AddedBy { get; set; }
    public virtual ICollection<ListTicketsByParticipant> Tickets { get; set; } = new List<ListTicketsByParticipant>();
}