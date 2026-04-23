using AutoMapper;
using FluentResults;
using RaffleHub.Api.DTOs.Gallery;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services.Interface;

namespace RaffleHub.Api.Services;

public class GalleryService
{
    private readonly IGalleryRepository _repository;
    private readonly ISupabaseService _supabaseService;
    private readonly IMapper _mapper;
    
    public GalleryService(IGalleryRepository repository, IMapper mapper, ISupabaseService supabaseService)
    {
        _repository = repository;
        _mapper = mapper;
        _supabaseService =  supabaseService;
    }
    
    public async Task<Result<IEnumerable<ListAllGalleryDto>>> ListAll(int page = 1, int pageSize = 10)
    {
        var result = await _repository.FindAllPaginated<ListAllGalleryDto>(_mapper.ConfigurationProvider, page, pageSize);
        
        return Result.Ok<IEnumerable<ListAllGalleryDto>>(result);
    }
    public async Task<Result<IEnumerable<ListAllGalleryDto>>> ListByYears(IEnumerable<int> years, int page = 1, int pageSize = 10)
    {
        var result = await _repository.FindByYearsPaginated<ListAllGalleryDto>(_mapper.ConfigurationProvider, years, page, pageSize);
        
        return Result.Ok<IEnumerable<ListAllGalleryDto>>(result);
    }

    public async Task<Result<IEnumerable<ListAllGalleryDto>>> ListByCategory(Guid categoryId, int page = 1, int pageSize = 10)
    {
        var result = await _repository.FindByCategoryPaginated<ListAllGalleryDto>(_mapper.ConfigurationProvider, categoryId, page, pageSize);
        
        return Result.Ok<IEnumerable<ListAllGalleryDto>>(result);
    }

    public async Task<Result<ListAllGalleryDto>> GetById(Guid id)
    {
        var result = await _repository.GetByIdAsync(id);
        if (result == null)
            return Result.Fail("Imagem não encontrada.");
        
        var dto = _mapper.Map<ListAllGalleryDto>(result);
        
        return Result.Ok(dto);
    }
    
    public async Task<Result<Guid>> CreateGallery(IFormFile? file, CreateGalleryDto dto)
    {
        var gallery = _mapper.Map<Gallery>(dto);

        if (file != null)
        {
            var imageResult = await _supabaseService.CreateImage(file, gallery.FolderName);
            if (imageResult.IsFailed)
                return imageResult.ToResult();

            gallery.ImageUrl = imageResult.Value;
        }
        
        _repository.Add(gallery);
        await _repository.SaveChangesAsync();
        
        return Result.Ok<Guid>(gallery.Id);
    }

    public async Task<Result<Guid>> UpdateGallery(Guid id, IFormFile? file, UpdateGalleryDto dto)
    {
        var gallery = await _repository.GetByIdAsync(id);
        if (gallery == null)
            return Result.Fail("Dados não encontrados");

        _mapper.Map(dto, gallery);

        if (file != null)
        {
            if (!string.IsNullOrEmpty(gallery.ImageUrl))
            {
                await _supabaseService.DeleteImageAsync(gallery.ImageUrl);
            }

            var imageResult = await _supabaseService.CreateImage(file, gallery.FolderName);
            if (imageResult.IsFailed)
                return imageResult.ToResult();

            gallery.ImageUrl = imageResult.Value;
        }

        _repository.Update(gallery);
        await _repository.SaveChangesAsync();
        
        return Result.Ok(gallery.Id);
    }

    public async Task<Result> DeleteGallery(Guid id)
    {
        var gallery = await _repository.GetByIdAsync(id);
        if (gallery == null)
            return Result.Fail("Dados não encontrados");
        
        _repository.Delete(gallery);
        await _repository.SaveChangesAsync();
        
        return Result.Ok();
    }
}