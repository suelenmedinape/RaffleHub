namespace RaffleHub.Api.DTOs.Dashboard;

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalTicketsSold { get; set; }
    public int ActiveRafflesCount { get; set; }
    public List<RecentSaleDto> RecentSales { get; set; } = new();
}

public class RecentSaleDto
{
    public Guid BookingId { get; set; }
    public string ParticipantName { get; set; }
    public string RaffleName { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
}
