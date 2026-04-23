using AutoMapper;
using FluentResults;
using RaffleHub.Api.DTOs.CategoriesGallery;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Services;

public class CategoriesGalleryService
{
    private readonly ICategoriesGalleryRepository _repository;
    private readonly IMapper _mapper;
    
    public CategoriesGalleryService(ICategoriesGalleryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<ListAllCategoriesDto>>> ListAll()
    {
        var result = await _repository.ListAll();
        var dto = _mapper.Map<List<ListAllCategoriesDto>>(result);
        return Result.Ok(dto);
    }
    
    public async Task<Result<ListAllCategoriesDto>> GetById(Guid id)
    {
        var result = await _repository.GetById(id);
        if (result == null)
            return Result.Fail("Categoria não encontrada.");
        
        var dto = _mapper.Map<ListAllCategoriesDto>(result);
        
        return Result.Ok(dto);
    }

    public async Task<Result<Guid>> CreateCategory(CreateCategoriesGalleryDto dto)
    {
        var category = _mapper.Map<CategoriesGallery>(dto);
        _repository.Add(category);
        await _repository.SaveChangesAsync();
        
        return Result.Ok(category.Id);
    }

    public async Task<Result<Guid>> UpdateCategory(Guid id, UpdateCategoriesGalleryDto dto)
    {
        var category = await _repository.GetById(id);
        if (category == null)
            return Result.Fail("Categoria não encontrada.");

        _mapper.Map(dto, category);
        
        _repository.Update(category);
        await _repository.SaveChangesAsync();

        return Result.Ok(category.Id);
    }
    
    public async Task<Result> DeleteCategory(Guid id)
    {
        var raffle = await _repository.GetById(id);
        if (raffle == null)
            return Result.Fail("Categoria não encontrada.");

        _repository.Delete(raffle);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}