using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.Utils.ExceptionHandler;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaffleHub.Api.Services;
using RaffleHub.Api.Repositories;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;

namespace RaffleHub.Tests.E2E;

public class TicketE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly string DbName = Guid.NewGuid().ToString();

    public TicketE2ETests(WebApplicationFactory<Program> factory)
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

                services.TryAddScoped<ITicketRepository, TicketRepository>();
                services.TryAddScoped<IRaffleRepository, RaffleRepository>();
                services.TryAddScoped<IParticipantRepository, ParticipantRepository>();
                services.TryAddScoped<TicketService>();
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");
    }

    private async Task<Guid> SeedData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var raffle = new Raffle 
        { Id = Guid.NewGuid(), RaffleName = "Rifa Teste", Description = "Desc", DrawDate = DateTime.UtcNow.AddDays(7), TotalTickets = 100, TicketPrice = 10 };
        context.Raffle.Add(raffle);

        var participant = new Participant { ParticipantName = "Test", Phone = "123", Cpf = "123", Id = Guid.NewGuid() };
        context.Participant.Add(participant);

        context.Ticket.AddRange(
            new Ticket { Raffle = null!, Id = Guid.NewGuid(), RaffleId = raffle.Id, TicketNumber = 1, ParticipantId = participant.Id },
            new Ticket { Raffle = null!, Id = Guid.NewGuid(), RaffleId = raffle.Id, TicketNumber = 2, ParticipantId = participant.Id },
            new Ticket { Raffle = null!, Id = Guid.NewGuid(), RaffleId = raffle.Id, TicketNumber = 3, ParticipantId = null } // Not sold
        );
        
        await context.SaveChangesAsync();
        return raffle.Id;
    }

    [Fact]
    public async Task ListTicketsSold_ShouldReturnOk_WithSoldNumbers()
    {
        // Arrange
        var raffleId = await SeedData();

        // Act
        var response = await _client.GetAsync($"/Ticket/{raffleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<int>>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Data!.Count);
        Assert.Contains(1, result.Data);
        Assert.Contains(2, result.Data);
        Assert.DoesNotContain(3, result.Data);
    }
}
