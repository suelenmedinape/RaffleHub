using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.Services;
using RaffleHub.Api.Utils.Extensions;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class TicketController : ControllerBase
{
    private readonly TicketService _service;

    public TicketController(TicketService service)
    {
        _service = service;
    }
    
    [HttpGet("{raffleId}")]
    public async Task<IActionResult> ListTicketsSold(Guid raffleId) => (await _service.ListTicketsSold(raffleId)).ToResponse();
}