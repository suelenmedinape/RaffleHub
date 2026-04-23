using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Repositories.Interfaces;

public interface ITicketRepository
{
    IQueryable<Ticket> GetQueryable();
    Task<int> GetMaxTicketNumber(Guid raffleId);
    Task<bool> IsTicketSold(Guid raffleId, int ticketNumber);
    void Delete(Ticket ticket);
}