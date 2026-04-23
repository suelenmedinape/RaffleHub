using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.DTOs.Ticket;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Services;

public class TicketService
{
    private readonly ITicketRepository _repository;
    private readonly IRaffleRepository _raffleRepository;
    private readonly IMapper _mapper;

    public TicketService(ITicketRepository repository, IRaffleRepository raffleRepository, IMapper mapper)
    {
        _repository = repository;
        _raffleRepository = raffleRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<int>>> ListTicketsSold(Guid raffleId)
    {
        var raffle = await _raffleRepository.GetByIdAsync(raffleId);
        if (raffle == null)
            return Result.Fail<List<int>>("Rifa não encontrada");

        var ticketNumbers = await _repository.GetQueryable()
            .Where(r => r.RaffleId == raffleId && r.ParticipantId != null)
            .OrderBy(r => r.TicketNumber)
            .Select(r => r.TicketNumber)
            .ToListAsync();

        return Result.Ok(ticketNumbers);
    }
}