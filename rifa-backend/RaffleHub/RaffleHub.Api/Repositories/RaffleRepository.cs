using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Repositories;

public class RaffleRepository : IRaffleRepository
{
    private readonly AppDbContext context;

    public RaffleRepository(AppDbContext context)
    {
        this.context = context;
    }
    
    public async Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig)
    {
        return await context.Set<Raffle>()
            .AsNoTracking()
            .ProjectTo<TResult>(mapperConfig)
            .ToListAsync();
    }
    
    public async Task<List<TResult>> GetById<TResult>(Guid id, AutoMapper.IConfigurationProvider mapperConfig)
    {
        return await context.Set<Raffle>()
            .AsNoTracking()
            .Where(x => EF.Property<Guid>(x, "Id") == id)
            .ProjectTo<TResult>(mapperConfig)
            .ToListAsync();
    }
    
    public async Task<Raffle?> GetByIdAsync(Guid id)
    {
        return await context.Raffle
            .Include(r => r.Tickets)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public IQueryable<Raffle> GetQueryable()
    {
        return context.Set<Raffle>().AsQueryable();
    }

    public void Add(Raffle raffle) => context.Raffle.Add(raffle);

    public void Update(Raffle raffle) => context.Raffle.Update(raffle);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}