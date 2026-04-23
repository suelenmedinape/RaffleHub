using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.DTOs.Dashboard;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Services;

public class DashboardService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IRaffleRepository _raffleRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IBookingRepository bookingRepository,
        IParticipantRepository participantRepository,
        IRaffleRepository raffleRepository,
        ILogger<DashboardService> logger)
    {
        _bookingRepository = bookingRepository;
        _participantRepository = participantRepository;
        _raffleRepository = raffleRepository;
        _logger = logger;
    }

    public async Task<FluentResults.Result<DashboardStatsDto>> GetGeneralStatsAsync()
    {
        try
        {
            var paidBookings = await _bookingRepository.GetQueryable()
                .Include(b => b.Tickets)
                .Include(b => b.Participant)
                .Include(b => b.Raffle)
                .Where(b => b.Status == BookingStatus.PAID)
                .ToListAsync();

            var stats = new DashboardStatsDto
            {
                TotalRevenue = paidBookings.Sum(b => b.TotalAmount),
                TotalTicketsSold = paidBookings.Sum(b => b.Tickets.Count),
                TotalParticipants = await _participantRepository.GetQueryable().CountAsync(),
                ActiveRafflesCount = await _raffleRepository.GetQueryable()
                    .CountAsync(r => r.Status == RaffleStatus.ACTIVE),
                
                RecentSales = paidBookings
                    .OrderByDescending(b => b.PaidAt)
                    .Take(10)
                    .Select(b => new RecentSaleDto
                    {
                        BookingId = b.Id,
                        ParticipantName = b.Participant?.ParticipantName ?? "N/A",
                        RaffleName = b.Raffle?.RaffleName ?? "N/A",
                        Amount = b.TotalAmount,
                        PaidAt = b.PaidAt ?? b.CreatedAt
                    })
                    .ToList()
            };

            return FluentResults.Result.Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular métricas do dashboard");
            return FluentResults.Result.Fail("Erro ao carregar dados do dashboard.");
        }
    }
}
