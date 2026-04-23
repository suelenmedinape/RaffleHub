using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.Booking;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Utils.ExceptionHandler;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Moq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaffleHub.Api.Services;
using RaffleHub.Api.Services.MercadoPago;
using RaffleHub.Api.Services.Interface;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Repositories;
using FluentResults;
using Microsoft.AspNetCore.Hosting;

namespace RaffleHub.Tests.E2E;

public class BookingE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly string DbName = Guid.NewGuid().ToString();
    private readonly Mock<IMercadoPagoMockService> _misticPayMock = new();

    public interface IMercadoPagoMockService : IMercadoPagoService { }

    public BookingE2ETests(WebApplicationFactory<Program> factory)
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

                services.RemoveAll<IMercadoPagoService>();
                _misticPayMock.Setup(s => s.CreatePixTransactionAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(Result.Ok(new MercadoPagoPixResponseDto { TransactionId = "tx123", PixCopyPaste = "copy", PixQrCodeUrl = "url" }));
                services.AddSingleton<IMercadoPagoService>(_misticPayMock.Object);

                services.TryAddScoped<IBookingRepository, BookingRepository>();
                services.TryAddScoped<IParticipantRepository, ParticipantRepository>();
                services.TryAddScoped<IRaffleRepository, RaffleRepository>();
                services.TryAddScoped<BookingService>();
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");
    }

    private async Task<(Guid participantId, Guid bookingId)> SeedData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var raffle = new Raffle 
        { Id = Guid.NewGuid(), RaffleName = "Rifa Teste", TotalTickets = 100, TicketPrice = 10, Description = "Descrição Teste" };
        context.Raffle.Add(raffle);

        var participant = new Participant { ParticipantName = "Test", Phone = "123", Cpf = "123", Id = Guid.NewGuid(), UserId = "test-user-id" }; 
        context.Participant.Add(participant);

        var booking = new Booking
        { Id = Guid.NewGuid(), RaffleId = raffle.Id, ParticipantId = participant.Id, TotalAmount = 100, Status = BookingStatus.PENDING, CreatedAt = DateTime.UtcNow };
        context.Bookings.Add(booking);
        
        await context.SaveChangesAsync();
        return (participant.Id, booking.Id);
    }

    [Fact]
    public async Task GetMyBookings_ShouldReturnOk()
    {
        await SeedData();
        var response = await _client.GetAsync("/Booking/my-bookings");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<MyBookingsDto>>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data!);
    }

    [Fact]
    public async Task BookingPending_ShouldReturnOk_WhenHasPending()
    {
        var (participantId, _) = await SeedData();
        var response = await _client.GetAsync($"/Booking/pending/{participantId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePixPayment_ShouldReturnOk()
    {
        var (participantId, _) = await SeedData();
        var response = await _client.PostAsync($"/Booking/generate-pix/{participantId}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ListBookingPendingDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Data!.PixCopyPaste);
    }
}
