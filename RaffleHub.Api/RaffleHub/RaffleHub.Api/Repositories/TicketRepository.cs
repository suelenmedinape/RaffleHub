using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext context;

    public TicketRepository(AppDbContext context)
    {
        this.context = context;
    }
    
    public IQueryable<Ticket> GetQueryable()
    {
        return context.Set<Ticket>().AsQueryable();
    }
    
    public async Task<int> GetMaxTicketNumber(Guid raffleId)
    {
        return await context.Set<Ticket>()
            .Where(t => t.RaffleId == raffleId)
            .MaxAsync(t => (int?)t.TicketNumber) ?? 0;
    }

    public async Task<bool> IsTicketSold(Guid raffleId, int ticketNumber)
    {
        return await context.Set<Ticket>()
            .AnyAsync(t => t.RaffleId == raffleId && t.TicketNumber == ticketNumber);
    }
    
    public void Delete(Ticket ticket) => context.Ticket.Remove(ticket);
}