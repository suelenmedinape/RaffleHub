using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaffleHub.Api.DTOs.Auth;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Api.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IParticipantRepository _participantRepository;

    public AuthService(
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IConfiguration configuration,
        IParticipantRepository participantRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _participantRepository = participantRepository;
    }

    public async Task<Result<AuthResponseDto>> RegisterUserAsync(RegisterDto dto, ClaimsPrincipal? currentUser = null)
    {
        var userExists = await _userManager.FindByEmailAsync(dto.Email);
        if (userExists != null)
            return Result.Fail("Já existe um usuário com esse e-mail.");

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var phoneExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == dto.Phone);
            if (phoneExists)
                return Result.Fail("Já existe um usuário cadastrado com este telefone.");
        }

        var user = new ApplicationUser
        {
            Email = dto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.Phone
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail($"Falha na criação: {errors}");
        }

        string roleToAssign = "PARTICIPANT";

        if (currentUser != null && currentUser.Identity?.IsAuthenticated == true && (currentUser.IsInRole("ADMIN") || currentUser.IsInRole("Admin")))
        {
            roleToAssign = "OPERATOR";
        }

        if (!await _roleManager.RoleExistsAsync(roleToAssign))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
        }

        await _userManager.AddToRoleAsync(user, roleToAssign);

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var sanitizedPhone = new string(dto.Phone.Where(char.IsDigit).ToArray());
            var participantsToLink = await _participantRepository.GetQueryable()
                .Where(p => p.Phone == sanitizedPhone && p.UserId == null)
                .ToListAsync();

            if (participantsToLink.Any())
            {
                foreach (var p in participantsToLink)
                {
                    p.UserId = user.Id;
                    _participantRepository.Update(p);
                }
                await _participantRepository.SaveChangesAsync();
            }
        }

        return await LoginAsync(new LoginDto { Email = dto.Email, Password = dto.Password });
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Result.Fail("Usuário não encontrado ou senha inválida.");

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
            new Claim("FullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = GenerateJwtToken(authClaims);
        var refreshToken = GenerateRefreshToken();

        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"] ?? "7", out int refreshTokenValidityInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenValidityInDays);

        await _userManager.UpdateAsync(user);

        return Result.Ok(new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Phone = user.PhoneNumber!,
            FullName = user.FullName,
            Roles = userRoles,
            Expiration = token.ValidTo
        });
    }

    private JwtSecurityToken GenerateJwtToken(List<Claim> authClaims)
    {
        var secret = _configuration["JWT:SecretKey"];
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured. Fail Fast for security.");
        }
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.UtcNow.AddHours(24),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(TokenApiModel tokenModel)
    {
        if (tokenModel is null || string.IsNullOrEmpty(tokenModel.AccessToken) || string.IsNullOrEmpty(tokenModel.RefreshToken))
        {
            return Result.Fail("Requisição de cliente inválida.");
        }

        string accessToken = tokenModel.AccessToken;
        string refreshToken = tokenModel.RefreshToken;

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return Result.Fail("Access token ou refresh token inválido.");
        }

        string username = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value 
                   ?? principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value 
                   ?? string.Empty;

        var user = await _userManager.FindByEmailAsync(username);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result.Fail("Access token ou refresh token inválido.");
        }

        var newAccessToken = GenerateJwtToken(principal.Claims.ToList());
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        var userRoles = await _userManager.GetRolesAsync(user);

        return Result.Ok(new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            RefreshToken = newRefreshToken,
            Phone = user.PhoneNumber!,
            FullName = user.FullName,
            Roles = userRoles,
            Expiration = newAccessToken.ValidTo
        });
    }

    public async Task<Result> RevokeAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return Result.Fail("Usuário inválido.");

        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);

        return Result.Ok();
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var secret = _configuration["JWT:SecretKey"];
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured. Fail Fast for security.");
        }
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Token inválido");

        return principal;
    }
}
