using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext context;

    public BookingRepository(AppDbContext context)
    {
        this.context = context;
    }
    
    public async Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig)
    {
        return await context.Set<Booking>()
            .AsNoTracking()
            .ProjectTo<TResult>(mapperConfig)
            .ToListAsync();
    }
    
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await context.Bookings
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Booking?> GetByIdLockedAsync(Guid id)
    {
        return await context.Bookings
            .FromSqlRaw("SELECT * FROM \"Bookings\" WHERE \"Id\" = {0} FOR UPDATE", id)
            .Include(b => b.Tickets)
            .Include(b => b.Participant)
            .Include(b => b.Raffle)
            .FirstOrDefaultAsync();
    }

    public async Task<Booking?> GetByTransactionIdLockedAsync(string transactionId)
    {
        return await context.Bookings
            .FromSqlRaw("SELECT * FROM \"Bookings\" WHERE \"TransactionId\" = {0} FOR UPDATE", transactionId)
            .Include(b => b.Tickets)
            .Include(b => b.Participant)
            .Include(b => b.Raffle)
            .FirstOrDefaultAsync();
    }
    
    public IQueryable<Booking> GetQueryable()
    {
        return context.Set<Booking>().AsQueryable();
    }

    public void Add(Booking booking) => context.Bookings.Add(booking);

    public void Update(Booking booking) => context.Bookings.Update(booking);
    
    public void Delete(Booking booking) => context.Bookings.Remove(booking);

    public async Task BeginTransactionAsync() 
    {
        if (context.Database.CurrentTransaction == null)
            await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync() 
    {
        if (context.Database.CurrentTransaction != null)
            await context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync() 
    {
        if (context.Database.CurrentTransaction != null)
            await context.Database.RollbackTransactionAsync();
    }

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}