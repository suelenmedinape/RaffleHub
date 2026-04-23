using AutoMapper;
using FluentResults;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services.Interface;

namespace RaffleHub.Api.Services;

public class RaffleService
{
    private readonly IRaffleRepository _repository;
    private readonly IMapper _mapper;
    private readonly ISupabaseService _supabaseService;

    public RaffleService(IRaffleRepository repository, IMapper mapper, ISupabaseService supabaseService)
    {
        _repository = repository;
        _mapper = mapper;
        _supabaseService =  supabaseService;
    }

    public async Task<Result<IEnumerable<ListAllRaffleDto>>> ListAll()
    {
        var result = await _repository.ListAll<ListAllRaffleDto>(_mapper.ConfigurationProvider);
        return Result.Ok<IEnumerable<ListAllRaffleDto>>(result);
    }

    public async Task<Result<IEnumerable<ListNamesRafflesDto>>> ListNamesRaffle()
    {
        var result = await _repository.ListAll<ListNamesRafflesDto>(_mapper.ConfigurationProvider);
        return Result.Ok<IEnumerable<ListNamesRafflesDto>>(result);
    }

    public async Task<Result<ListAllRaffleDto>> GetById(Guid id)
    {
        var result = await _repository.GetByIdAsync(id);
        if (result == null)
            return Result.Fail("Rifa não encontrada.");
        
        var dto = _mapper.Map<ListAllRaffleDto>(result);
        
        return Result.Ok(dto);
    }
    
    public async Task<Result<Guid>> CreateRaffle(IFormFile? file, CreateRaffleDto dto)
    {
        var raffle = _mapper.Map<Raffle>(dto);
        
        if (file != null)
        {
            var imageResult = await _supabaseService.CreateImage(file, raffle.FolderName);
            if (imageResult.IsFailed)
                return imageResult.ToResult();
                
            raffle.ImageUrl = imageResult.Value;
        }

        _repository.Add(raffle);
        await _repository.SaveChangesAsync();

        return Result.Ok(raffle.Id);
    }
    
    public async Task<Result<Guid>> UpdateRaffle(Guid id, IFormFile? file, UpdateRaffleDto dto)
    {
        var raffle = await _repository.GetByIdAsync(id);

        if (raffle == null)
            return Result.Fail("Rifa não encontrada.");

        if (dto.TotalTickets < raffle.TotalTickets)
            return Result.Fail(
                $"Não é possível reduzir o número total de bilhetes. O valor atual é {raffle.TotalTickets}.");

        _mapper.Map(dto, raffle);

        if (file != null)
        {
            if (!string.IsNullOrEmpty(raffle.ImageUrl))
            {
                await _supabaseService.DeleteImageAsync(raffle.ImageUrl);
            }

            var imageResult = await _supabaseService.CreateImage(file, raffle.FolderName);
            if (imageResult.IsFailed)
                return imageResult.ToResult();

            raffle.ImageUrl = imageResult.Value;
        }

        _repository.Update(raffle);
        await _repository.SaveChangesAsync();

        return Result.Ok(raffle.Id);
    }
    
    public async Task<Result<string>> ChangeRaffleStatus(Guid id, UpdateStatusDto dto)
    {
        var raffle = await _repository.GetByIdAsync(id);
        if (raffle == null)
            return Result.Fail("Rifa não encontrada.");

        if (raffle.Status == RaffleStatus.EXPIRED && raffle.DrawDate < DateTime.UtcNow)
            return Result.Fail("Rifa expirada. Edite a data do sorteio antes de reativá-la.");

        if (raffle.Status == RaffleStatus.CANCELLED)
            return Result.Fail("Rifa cancelada não pode ter o status alterado.");

        if (raffle.Status == RaffleStatus.COMPLETED)
            return Result.Fail("Rifa já finalizada não pode ter o status alterado.");

        if (raffle.Status == dto.Status)
            return Result.Fail($"A rifa já está com o status '{dto.Status}'.");

        if (dto.Status == RaffleStatus.ACTIVE && raffle.DrawDate < DateTime.UtcNow)
            return Result.Fail("Não é possível ativar uma rifa com data de sorteio no passado.");

        raffle.Status = dto.Status;
        await _repository.SaveChangesAsync();

        return Result.Ok($"Rifa {dto.Status} com sucesso");
    }
}