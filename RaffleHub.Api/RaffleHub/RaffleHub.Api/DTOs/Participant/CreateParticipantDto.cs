namespace RaffleHub.Api.DTOs.Participant;

public class CreateParticipantDto
{
    public string ParticipantName { get; set; }
    public string Phone {  get; set; }
    public string Document { get; set; }
    
    public Guid RaffleId { get; set; }
    public List<int> TicketNumbers { get; set; } = new();
}