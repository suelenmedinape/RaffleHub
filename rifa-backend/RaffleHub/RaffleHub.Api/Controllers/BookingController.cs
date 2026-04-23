using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.Services;
using RaffleHub.Api.Utils.Extensions;

using Microsoft.AspNetCore.RateLimiting;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class BookingController : ControllerBase
{
    private readonly BookingService _service;

    public BookingController(BookingService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings() =>
        (await _service.GetMyBookingsAsync()).ToResponse();

    [EnableRateLimiting("PixLimit")]
    [HttpGet("pending/{participantId}")]
    public async Task<IActionResult> GetPending(Guid participantId) =>
        (await _service.BookingPending(participantId)).ToResponse();

    [EnableRateLimiting("PixLimit")]
    [HttpPost("generate-pix/{participantId}")]
    public async Task<IActionResult> GeneratePix(Guid participantId) =>
        (await _service.GeneratePixPaymentAsync(participantId)).ToResponse();

    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpPatch("confirm-manual/{bookingId}")]
    public async Task<IActionResult> ConfirmManual(Guid bookingId) =>
        (await _service.ConfirmManualPaymentAsync(bookingId)).ToResponse("Pagamento confirmado manualmente com sucesso!");
}
