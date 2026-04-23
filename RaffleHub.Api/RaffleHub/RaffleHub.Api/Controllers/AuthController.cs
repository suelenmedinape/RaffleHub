using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.DTOs.Auth;
using RaffleHub.Api.Services;
using RaffleHub.Api.Utils.Extensions;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model) =>
        (await _authService.LoginAsync(model)).ToResponse();

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model) =>
        (await _authService.RegisterUserAsync(model, User)).ToResponse();

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenApiModel tokenModel) =>
        (await _authService.RefreshTokenAsync(tokenModel)).ToResponse();

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] string email) =>
        (await _authService.RevokeAsync(email)).ToResponse();
}
