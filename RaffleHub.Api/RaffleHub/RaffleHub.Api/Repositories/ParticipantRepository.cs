using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly AppDbContext context;

    public ParticipantRepository(AppDbContext context)
    {
        this.context = context;
    }
    
    public async Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig)
    {
        return await context.Set<Participant>()
            .AsNoTracking()
            .ProjectTo<TResult>(mapperConfig)
            .ToListAsync();
    }
    
    public async Task<List<TResult>> GetById<TResult>(Guid id, AutoMapper.IConfigurationProvider mapperConfig)
    {
        return await context.Set<Participant>()
            .AsNoTracking()
            .Where(x => EF.Property<Guid>(x, "Id") == id)
            .ProjectTo<TResult>(mapperConfig)
            .ToListAsync();
    }
    
    public async Task<Participant?> GetByIdAsync(Guid id)
    {
        return await context.Participant
            .Include(p => p.Tickets)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public IQueryable<Participant> GetQueryable()
    {
        return context.Set<Participant>().AsQueryable();
    }

    public void Add(Participant participant) => context.Participant.Add(participant);

    public void Update(Participant participant) => context.Participant.Update(participant);

    public void Delete(Participant participant) => context.Participant.Remove(participant);
    
    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}