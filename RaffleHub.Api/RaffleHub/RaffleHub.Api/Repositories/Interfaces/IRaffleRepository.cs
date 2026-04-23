using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Repositories.Interfaces;

public interface IRaffleRepository
{
    Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig);
    Task<List<TResult>> GetById<TResult>(Guid id, AutoMapper.IConfigurationProvider mapperConfig);
    Task<Raffle?> GetByIdAsync(Guid id);
    IQueryable<Raffle> GetQueryable();
    void Add(Raffle raffle);
    void Update(Raffle raffle);
    Task SaveChangesAsync();
}