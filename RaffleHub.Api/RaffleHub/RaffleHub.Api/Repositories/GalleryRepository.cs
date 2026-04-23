using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Repositories;

public class GalleryRepository : IGalleryRepository
{
    private readonly AppDbContext context;

    public GalleryRepository(AppDbContext context)
    {
        this.context = context;
    }
    
    /*public async Task<Galley?> GetByIdAsync(Guid id)
    {
        return await context.Raffle
            .Include(r => r.Tickets)
            .FirstOrDefaultAsync(r => r.Id == id);
    }*/
    
    public async Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig)
    {
        return await context.Set<Gallery>()
            .AsNoTracking()
            .ProjectTo<TResult>(mapperConfig)
            .ToListAsync();
    }
    
    public async Task<List<TResult>> FindAllPaginated<TResult>(AutoMapper.IConfigurationProvider mapperConfig, int page, int pageSize)
    {
        return await context.Set<Gallery>()
            .AsNoTracking()
            .ProjectTo<TResult>(mapperConfig)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    public async Task<List<TResult>> FindByYearsPaginated<TResult>(AutoMapper.IConfigurationProvider mapperConfig, IEnumerable<int> years, int page, int pageSize)
    {
        var query = context.Set<Gallery>().AsNoTracking();
        
        if (years != null && years.Any())
        {
            query = query.Where(g => years.Contains(g.Year));
        }

        return await query
            .ProjectTo<TResult>(mapperConfig)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<List<TResult>> FindByCategoryPaginated<TResult>(AutoMapper.IConfigurationProvider mapperConfig, Guid categoryId, int page, int pageSize)
    {
        return await context.Set<Gallery>()
            .AsNoTracking()
            .Where(g => g.CategoryId == categoryId)
            .ProjectTo<TResult>(mapperConfig)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Gallery?> GetByIdAsync(Guid id)
    {
        return await context.Gallery
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public void Add(Gallery gallery) => context.Gallery.Add(gallery);

    public void Update(Gallery gallery) => context.Gallery.Update(gallery);
    
    public void Delete(Gallery gallery) => context.Gallery.Remove(gallery);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}