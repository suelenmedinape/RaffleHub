using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig);
    Task<Booking?> GetByIdAsync(Guid id);
    Task<Booking?> GetByIdLockedAsync(Guid id);
    Task<Booking?> GetByTransactionIdLockedAsync(string transactionId);
    IQueryable<Booking> GetQueryable();
    void Add(Booking booking);
    void Update(Booking booking);
    void Delete(Booking booking);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task SaveChangesAsync();
}