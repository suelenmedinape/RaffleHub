using RaffleHub.Api.Enums;

namespace RaffleHub.Api.Entities;

public class Raffle
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; } =
        "https://tcjbiwxyvozqnajhauro.supabase.co/storage/v1/object/public/storage-iff-road/raffle/image_not_found.png";
    public string FolderName { get; set; } = "raffle";
    public string RaffleName { get; set; }
    public string Description { get; set; }
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
    public DateTime DrawDate  { get; set; }
    public RaffleStatus Status { get; set; } = RaffleStatus.ACTIVE;
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}