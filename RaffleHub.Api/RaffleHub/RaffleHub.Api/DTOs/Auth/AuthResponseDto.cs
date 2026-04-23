namespace RaffleHub.Api.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime Expiration { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
