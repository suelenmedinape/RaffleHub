using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Services;
using RaffleHub.Api.Utils.Extensions;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class RaffleController : ControllerBase
{
    private readonly RaffleService _service;
    
    public RaffleController(RaffleService service)
    {
        _service = service;
    }
    
    [HttpGet]
    public async Task<IActionResult> ListAll() => 
        (await _service.ListAll()).ToResponse();
    
    [HttpGet("names")]
    public async Task<IActionResult> ListNamesRaffle() => 
        (await _service.ListNamesRaffle()).ToResponse();
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) => 
        (await _service.GetById(id)).ToResponse();
    
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> NewRaffle(IFormFile? file, [FromForm] CreateRaffleDto dto)
    {
        var result = await _service.CreateRaffle(file, dto);
        return result.ToCreatedResponse(nameof(GetById), new { id = result.Value }, "Rifa criada com sucesso!");
    }
    
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRaffle(Guid id, IFormFile? file, [FromForm] UpdateRaffleDto dto) =>
        (await _service.UpdateRaffle(id, file, dto)).ToResponse("Rifa atualizada com sucesso!");
    
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("ChangeStatus/{id}")]
    public async Task<IActionResult> ChangeRaffleStatus(Guid id, UpdateStatusDto dto) =>
        (await _service.ChangeRaffleStatus(id, dto)).ToResponse();
}