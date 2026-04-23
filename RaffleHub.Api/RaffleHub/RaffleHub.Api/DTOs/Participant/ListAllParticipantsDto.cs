namespace RaffleHub.Api.DTOs.Participant;

public class ListAllParticipantsDto
{
    public Guid Id { get; set; }
    public string ParticipantName { get; set; }
    public string Phone { get; set; }
    
    //public Guid AddedBy { get; set; }
    public ICollection<int> Tickets { get; set; } = new List<int>();
}