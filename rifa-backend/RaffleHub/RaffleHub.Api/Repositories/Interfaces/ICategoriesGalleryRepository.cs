using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Repositories.Interfaces;

public interface ICategoriesGalleryRepository
{
    Task<List<CategoriesGallery>> ListAll();
    Task<CategoriesGallery?> GetById(Guid id);
    void Add(CategoriesGallery categoriesGallery);
    void Update(CategoriesGallery categoriesGallery);
    void Delete(CategoriesGallery categoriesGallery);
    Task SaveChangesAsync();
}