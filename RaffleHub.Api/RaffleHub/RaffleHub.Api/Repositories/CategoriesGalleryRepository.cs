using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.CategoriesGallery;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Repositories;

public class CategoriesGalleryRepository : ICategoriesGalleryRepository
{
    private readonly AppDbContext context;

    public CategoriesGalleryRepository(AppDbContext context)
    {
        this.context = context;
    }
    
    /*public async Task<Galley?> GetByIdAsync(Guid id)
    {
        return await context.Raffle
            .Include(r => r.Tickets)
            .FirstOrDefaultAsync(r => r.Id == id);
    }*/

    public async Task<List<CategoriesGallery>> ListAll()
    {
        return await context.CategoriesGallery.ToListAsync();
    }

    public async Task<CategoriesGallery?> GetById(Guid id)
    {
        return await context.CategoriesGallery.FindAsync(id);
    }
    
    public void Add(CategoriesGallery categoriesGallery) => context.CategoriesGallery.Add(categoriesGallery);

    public void Update(CategoriesGallery categoriesGallery) => context.CategoriesGallery.Update(categoriesGallery);
    
    public void Delete(CategoriesGallery categoriesGallery) => context.CategoriesGallery.Remove(categoriesGallery);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}