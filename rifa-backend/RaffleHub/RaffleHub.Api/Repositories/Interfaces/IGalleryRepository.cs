using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Repositories.Interfaces;

public interface IGalleryRepository
{
    Task<List<TResult>> ListAll<TResult>(AutoMapper.IConfigurationProvider mapperConfig);
    Task<List<TResult>> FindAllPaginated<TResult>(AutoMapper.IConfigurationProvider mapperConfig, int page, int pageSize);
    Task<List<TResult>> FindByYearsPaginated<TResult>(AutoMapper.IConfigurationProvider mapperConfig, IEnumerable<int> years, int page, int pageSize);
    Task<List<TResult>> FindByCategoryPaginated<TResult>(AutoMapper.IConfigurationProvider mapperConfig, Guid categoryId, int page, int pageSize);
    Task<Gallery?> GetByIdAsync(Guid id);
    void Add(Gallery gallery);
    void Update(Gallery gallery);
    void Delete(Gallery gallery);
    Task SaveChangesAsync();
}