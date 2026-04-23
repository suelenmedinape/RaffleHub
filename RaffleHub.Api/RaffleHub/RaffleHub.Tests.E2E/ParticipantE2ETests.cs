using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.Participant;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Utils.ExceptionHandler;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaffleHub.Api.Services;
using RaffleHub.Api.Repositories;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services.Interface;
using RaffleHub.Api.Entities;

using Microsoft.Data.Sqlite;

namespace RaffleHub.Tests.E2E;

public class ParticipantE2ETests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _sqliteConnection;

    public ParticipantE2ETests(WebApplicationFactory<Program> factory)
    {
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        _sqliteConnection.Open();

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
                    options.UseSqlite(_sqliteConnection);
                });

                services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = "TestAuth";
                    opts.DefaultChallengeScheme = "TestAuth";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });

                // Ensure repositories and services are correctly registered
                services.TryAddScoped<IRaffleRepository, RaffleRepository>();
                services.TryAddScoped<IParticipantRepository, ParticipantRepository>();
                services.TryAddScoped<ITicketRepository, TicketRepository>();
                services.TryAddScoped<IBookingRepository, BookingRepository>();
                services.TryAddScoped<ParticipantService>();
                services.TryAddScoped<RaffleService>();
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");
    }

    private async Task<Guid> SeedRaffle()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        var raffle = new Raffle
        { Id = Guid.NewGuid(), RaffleName = "Rifa E2E", Description = "Desc E2E", TotalTickets = 100, TicketPrice = 10, Status = RaffleStatus.ACTIVE, DrawDate = DateTime.UtcNow.AddDays(7) };
        context.Raffle.Add(raffle);
        await context.SaveChangesAsync();
        return raffle.Id;
    }



    [Fact]
    public async Task CreateParticipant_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var raffleId = await SeedRaffle();
        var dto = new CreateParticipantDto
        { ParticipantName = "Participante Teste", Phone = "11988887777", Document = "12345678900", RaffleId = raffleId, TicketNumbers = new List<int> { 1 }
        };
        var content = JsonContent.Create(dto);

        // Act
        var response = await _client.PostAsync("/Participant", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ParticipantPurchaseResponseDto>>();
        Assert.NotNull(result);
        Assert.Equal(dto.ParticipantName, result.Data!.ParticipantName);
        Assert.Equal(3, result.Data!.TicketNumbers.Count);
    }

    [Fact]
    public async Task ListByRaffle_ShouldReturnOk_WhenParticipantsExist()
    {
        // Arrange
        var raffleId = await SeedRaffle();
        var dto = new CreateParticipantDto
        { ParticipantName = "Participante List", Phone = "11999999999", Document = "00000000000", RaffleId = raffleId, TicketNumbers = new List<int> { 10 }
        };
        await _client.PostAsync("/Participant", JsonContent.Create(dto));

        // Act
        var response = await _client.GetAsync($"/Participant/raffle/{raffleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ListAllParticipantsDto>>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data!);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenParticipantExists()
    {
        // Arrange
        var raffleId = await SeedRaffle();
        var dto = new CreateParticipantDto
        { ParticipantName = "Participante Get", Phone = "11977777777", Document = "11111111111", RaffleId = raffleId, TicketNumbers = new List<int> { 20 }
        };
        var postResponse = await _client.PostAsync("/Participant", JsonContent.Create(dto));
        var createResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<ParticipantPurchaseResponseDto>>();
        var participantId = createResult!.Data!.ParticipantId;

        // Act
        var response = await _client.GetAsync($"/Participant/{participantId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ParticipantDetailDto>>();
        Assert.NotNull(result);
        Assert.Equal(participantId, result.Data!.Id);
    }

    [Fact]
    public async Task DeleteParticipant_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var raffleId = await SeedRaffle();
        var dto = new CreateParticipantDto
        { ParticipantName = "Participante Delete", Phone = "11966666666", Document = "22222222222", RaffleId = raffleId, TicketNumbers = new List<int> { 30 }
        };
        var postResponse = await _client.PostAsync("/Participant", JsonContent.Create(dto));
        var createResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<ParticipantPurchaseResponseDto>>();
        var participantId = createResult!.Data!.ParticipantId;

        // Act
        var response = await _client.DeleteAsync($"/Participant/{participantId}/raffle/{raffleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        Assert.Equal("Participante deletado com sucesso!", result!.Message);
    }

    [Fact]
    public async Task CreateParticipant_ConcurrentRequestsForSameTicket_ShouldAllowOnlyOne()
    {
        // Arrange
        var raffleId = await SeedRaffle();
        var ticketNumbers = new List<int> { 50 };
        
        var dto1 = new CreateParticipantDto
        { ParticipantName = "Participante Concorrente 1", Phone = "11911111111", Document = "11111111111", RaffleId = raffleId, TicketNumbers = ticketNumbers };
        var content1 = JsonContent.Create(dto1);

        var dto2 = new CreateParticipantDto
        { ParticipantName = "Participante Concorrente 2", Phone = "11922222222", Document = "22222222222", RaffleId = raffleId, TicketNumbers = ticketNumbers };
        var content2 = JsonContent.Create(dto2);

        var dto3 = new CreateParticipantDto
        { ParticipantName = "Participante Concorrente 3", Phone = "11933333333", Document = "33333333333", RaffleId = raffleId, TicketNumbers = ticketNumbers };
        var content3 = JsonContent.Create(dto3);

        var tasks = new List<Task<HttpResponseMessage>>
        {
            _client.PostAsync("/Participant", content1),
            _client.PostAsync("/Participant", content2),
            _client.PostAsync("/Participant", content3)
        };

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        var successCount = results.Count(r => r.IsSuccessStatusCode);
        var failedCount = results.Count(r => !r.IsSuccessStatusCode);

        if (successCount == 0)
        {
            var errors = new List<string>();
            foreach (var r in results)
            {
                var content = await r.Content.ReadAsStringAsync();
                errors.Add($"Status: {r.StatusCode}, Error: {content}");
            }
            Assert.Fail("All requests failed! Details: " + string.Join(" | ", errors));
        }

        Assert.Equal(1, successCount);
        Assert.Equal(2, failedCount);
    }

    public void Dispose()
    {
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();
        _client.Dispose();
    }
}
