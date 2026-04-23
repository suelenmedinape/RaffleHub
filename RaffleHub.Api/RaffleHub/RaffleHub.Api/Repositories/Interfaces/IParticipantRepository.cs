using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Repositories.Interfaces;

public interface IParticipantRepository
{
    Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig);
    Task<List<TResult>> GetById<TResult>(Guid id, AutoMapper.IConfigurationProvider mapperConfig);
    Task<Participant?> GetByIdAsync(Guid id);
    IQueryable<Participant> GetQueryable();
    void Add(Participant participant);
    void Update(Participant participant);
    void Delete(Participant participant);
    Task SaveChangesAsync();
}