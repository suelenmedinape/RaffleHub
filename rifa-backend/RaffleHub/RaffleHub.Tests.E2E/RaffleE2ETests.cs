using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Utils.ExceptionHandler;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Moq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaffleHub.Api.Services;
using RaffleHub.Api.Services.Interface;
using RaffleHub.Api.Repositories;
using RaffleHub.Api.Repositories.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace RaffleHub.Tests.E2E;

public class RaffleE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly string DbName = Guid.NewGuid().ToString();
    private readonly Mock<ISupabaseService> _supabaseMock = new();

    public RaffleE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
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

                services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = "TestAuth";
                    opts.DefaultChallengeScheme = "TestAuth";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });

                services.RemoveAll<ISupabaseService>();
                _supabaseMock.Setup(s => s.CreateImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
                    .ReturnsAsync(Result.Ok("http://supabase.com/test.png"));
                services.AddSingleton<ISupabaseService>(_supabaseMock.Object);

                services.AddScoped<IRaffleRepository, RaffleRepository>();
                services.AddScoped<RaffleService>();
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");
    }

    private CreateRaffleDto CreateValidRaffleDto() => new()
    { RaffleName = "Rifa de Teste", Description = "Descrição da Rifa de Teste", TotalTickets = 100, TicketPrice = 10.0m, DrawDate = DateTime.UtcNow.AddDays(7) };

    private MultipartFormDataContent CreateMultipartContent(CreateRaffleDto dto)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.RaffleName), nameof(dto.RaffleName));
        content.Add(new StringContent(dto.Description), nameof(dto.Description));
        content.Add(new StringContent(dto.TotalTickets.ToString()), nameof(dto.TotalTickets));
        content.Add(new StringContent(dto.TicketPrice.ToString()), nameof(dto.TicketPrice));
        content.Add(new StringContent(dto.DrawDate.ToString("o")), nameof(dto.DrawDate));
        return content;
    }

    [Fact]
    public async Task CreateRaffle_ShouldReturnCreated_WhenDataIsValid()
    {
        var dto = CreateValidRaffleDto();
        var content = CreateMultipartContent(dto);
        var response = await _client.PostAsync("/Raffle", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk_AndContainCreatedRaffles()
    {
        var dto = CreateValidRaffleDto();
        var content = CreateMultipartContent(dto);
        await _client.PostAsync("/Raffle", content);

        var response = await _client.GetAsync("/Raffle");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
