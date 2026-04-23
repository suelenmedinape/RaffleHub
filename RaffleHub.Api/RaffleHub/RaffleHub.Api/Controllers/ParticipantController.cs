using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RaffleHub.Api.DTOs.Participant;
using RaffleHub.Api.Services;
using RaffleHub.Api.Utils.Extensions;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ParticipantController : ControllerBase
{
    private readonly ParticipantService _service;

    public ParticipantController(ParticipantService service)
    {
        _service = service;
    }

    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpGet("Raffle/{raffleId}")]
    public async Task<IActionResult> ListByRaffle(Guid raffleId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? searchTerm = null) =>
        (await _service.ListByRaffle(raffleId, page, pageSize, searchTerm)).ToResponse();

    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) =>
        (await _service.GetById(id)).ToResponse();
    
    [EnableRateLimiting("PixLimit")]
    [HttpPost]
    public async Task<IActionResult> NewParticipant([FromBody] CreateParticipantDto dto)
    {
        var result = await _service.CreateParticipant(dto);
        return result.ToCreatedResponse(nameof(GetById), result.IsSuccess && result.Value != null ? new { id = result.Value.ParticipantId } : null, "Participante criado com sucesso!");
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{participantId}/{raffleId}")]
    public async Task<IActionResult> DeleteParticipant(Guid participantId, Guid raffleId) =>
        (await _service.DeleteParticipant(participantId, raffleId)).ToResponse("Participante deletado com sucesso!");
}
