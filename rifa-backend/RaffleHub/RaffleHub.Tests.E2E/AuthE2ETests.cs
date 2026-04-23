using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.Auth;
using RaffleHub.Api.Utils.ExceptionHandler;
using RaffleHub.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace RaffleHub.Tests.E2E;

public class AuthE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly string DbName = Guid.NewGuid().ToString();

    public AuthE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(
                    d => d.ServiceType.Name.Contains("DbContextOptions") || 
                         d.ServiceType == typeof(AppDbContext) ||
                         d.ServiceType.Name.Contains("IDatabaseProvider")).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DbName);
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var dto = new RegisterDto
        { Email = $"test_{Guid.NewGuid() }@test.com",
            Password = "Password123!",
            FullName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/Auth/register", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        Assert.NotNull(result);
        Assert.Equal(dto.Phone, result.Data!.Phone);
        Assert.NotNull(result.Data.Token);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid()}@test.com";
        var password = "Password123!";
        
        // Pre-register user
        await _client.PostAsJsonAsync("/Auth/register", new RegisterDto
        { Email = email, Password = password, FullName = "Login User" });

        var loginDto = new LoginDto { Email = email, Password = password };

        // Act
        var response = await _client.PostAsJsonAsync("/Auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Data!.Token);
    }
}
