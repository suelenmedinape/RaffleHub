using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RaffleHub.Api.DTOs.Participant;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services;
using MockQueryable.Moq;
using MockQueryable;
using FluentResults;

namespace RaffleHub.Tests;

public class ParticipantServiceTests
{
    private readonly Mock<IParticipantRepository> _participantRepoMock;
    private readonly Mock<IRaffleRepository> _raffleRepoMock;
    private readonly Mock<ITicketRepository> _ticketRepoMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ParticipantService _sut;

    public ParticipantServiceTests()
    {
        _participantRepoMock = new Mock<IParticipantRepository>();
        _raffleRepoMock = new Mock<IRaffleRepository>();
        _ticketRepoMock = new Mock<ITicketRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _mapperMock = new Mock<IMapper>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _sut = new ParticipantService(
            _participantRepoMock.Object,
            _raffleRepoMock.Object,
            _ticketRepoMock.Object,
            _bookingRepoMock.Object,
            _mapperMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task ListByRaffle_WhenRaffleNotFound_ShouldReturnFail()
    {
        // Arrange
        var raffleId = Guid.NewGuid();
        _raffleRepoMock.Setup(r => r.GetByIdAsync(raffleId)).ReturnsAsync((Raffle?)null);

        // Act
        var result = await _sut.ListByRaffle(raffleId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa não encontrada.");
    }

    [Fact]
    public async Task ListByRaffle_WhenNoParticipants_ShouldReturnFail()
    {
        // Arrange
        var raffleId = Guid.NewGuid();
        _raffleRepoMock.Setup(r => r.GetByIdAsync(raffleId)).ReturnsAsync(new Raffle { RaffleName = "Test", Description = "Desc", Id = raffleId });
        
        var tickets = new List<Ticket>().BuildMock();
        _ticketRepoMock.Setup(t => t.GetQueryable()).Returns(tickets);

        _participantRepoMock.Setup(p => p.GetQueryable()).Returns(new List<Participant>().BuildMock());
        _bookingRepoMock.Setup(b => b.GetQueryable()).Returns(new List<Booking>().BuildMock());

        // Act
        var result = await _sut.ListByRaffle(raffleId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Nenhum número foi vendido até o momento");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        _participantRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Participant?)null);

        // Act
        var result = await _sut.GetById(id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Participante não encontrada.");
    }

    [Fact]
    public async Task CreateParticipant_WhenRaffleNotFound_ShouldReturnFail()
    {
        // Arrange
        var dto = new CreateParticipantDto { RaffleId = Guid.NewGuid(), TicketNumbers = new List<int> { 1 }, ParticipantName = "Test", Phone = "123", Document = "123" };
        _raffleRepoMock.Setup(r => r.GetByIdAsync(dto.RaffleId)).ReturnsAsync((Raffle?)null);

        // Act
        var result = await _sut.CreateParticipant(dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa não encontrada.");
    }

    [Fact]
    public async Task CreateParticipant_WhenRaffleNotActive_ShouldReturnFail()
    {
        // Arrange
        var dto = new CreateParticipantDto { ParticipantName = "Test", Phone = "123", Document = "123", RaffleId = Guid.NewGuid(), TicketNumbers = new List<int> { 1 } };
        _raffleRepoMock.Setup(r => r.GetByIdAsync(dto.RaffleId))
            .ReturnsAsync(new Raffle { RaffleName = "Test", Description = "Desc", Status = RaffleStatus.COMPLETED });
        _ticketRepoMock.Setup(t => t.GetQueryable()).Returns(new List<Ticket>().BuildMock());

        // Act
        var result = await _sut.CreateParticipant(dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Não é mais permitido participar dessa rifa.");
    }

    [Fact]
    public async Task CreateParticipant_WhenTicketNumberInvalid_ShouldReturnFail()
    {
        // Arrange
        var raffleId = Guid.NewGuid();
        var dto = new CreateParticipantDto { ParticipantName = "Test", Phone = "123", Document = "123", RaffleId = raffleId, TicketNumbers = new List<int> { 150 } };
        _raffleRepoMock.Setup(r => r.GetByIdAsync(raffleId))
            .ReturnsAsync(new Raffle { RaffleName = "Test", Description = "Desc", Id = raffleId, Status = RaffleStatus.ACTIVE, TotalTickets = 100 });

        // Act
        var result = await _sut.CreateParticipant(dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "O número 150 é inválido para esta rifa.");
    }

    [Fact]
    public async Task CreateParticipant_WhenTicketAlreadySold_ShouldReturnFail()
    {
        // Arrange
        var raffleId = Guid.NewGuid();
        var dto = new CreateParticipantDto { ParticipantName = "Test", Phone = "123", Document = "123", RaffleId = raffleId, TicketNumbers = new List<int> { 50 } };
        _raffleRepoMock.Setup(r => r.GetByIdAsync(raffleId))
            .ReturnsAsync(new Raffle { RaffleName = "Test", Description = "Desc", Id = raffleId, Status = RaffleStatus.ACTIVE, TotalTickets = 100 });
        var soldTickets = new List<Ticket> { new Ticket { Raffle = null!, RaffleId = raffleId, TicketNumber = 50 } }.BuildMock();
        _ticketRepoMock.Setup(t => t.GetQueryable()).Returns(soldTickets);

        // Act
        var result = await _sut.CreateParticipant(dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Os seguintes números já estão reservados ou foram vendidos: 50");
    }

    [Fact]
    public async Task CreateParticipant_WhenHasPendingBookings_ShouldReturnFail()
    {
        // Arrange
        var raffleId = Guid.NewGuid();
        var dto = new CreateParticipantDto { ParticipantName = "Test", Phone = "123", Document = "123", RaffleId = raffleId, TicketNumbers = new List<int> { 10 } };
        var participantId = Guid.NewGuid();
        var participant = new Participant { ParticipantName = "Test", Phone = "123", Cpf = "123", Id = participantId };

        _raffleRepoMock.Setup(r => r.GetByIdAsync(raffleId))
            .ReturnsAsync(new Raffle { RaffleName = "Test", Description = "Desc", Id = raffleId, Status = RaffleStatus.ACTIVE, TotalTickets = 100, TicketPrice = 10 });
        
        var participants = new List<Participant> { participant }.BuildMock();
        _participantRepoMock.Setup(p => p.GetQueryable()).Returns(participants);

        var bookings = new List<Booking> { new Booking { Participant = null!, Raffle = null!, ParticipantId = participantId, Status = BookingStatus.PENDING } }.BuildMock();
        _bookingRepoMock.Setup(b => b.GetQueryable()).Returns(bookings);
        _ticketRepoMock.Setup(t => t.GetQueryable()).Returns(new List<Ticket>().BuildMock());

        // Act
        var result = await _sut.CreateParticipant(dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "É preciso pagar pela compra anterior para poder realizar uma nova compra");
    }

    [Fact]
    public async Task DeleteParticipant_WhenRaffleNotFound_ShouldReturnFail()
    {
        // Arrange
        var participantId = Guid.NewGuid();
        var raffleId = Guid.NewGuid();
        _raffleRepoMock.Setup(r => r.GetByIdAsync(raffleId)).ReturnsAsync((Raffle?)null);

        // Act
        var result = await _sut.DeleteParticipant(participantId, raffleId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa não encontrada.");
    }

    [Fact]
    public async Task DeleteParticipant_WhenParticipantNotFound_ShouldReturnFail()
    {
        // Arrange
        var participantId = Guid.NewGuid();
        var raffleId = Guid.NewGuid();
        _raffleRepoMock.Setup(r => r.GetByIdAsync(raffleId)).ReturnsAsync(new Raffle { RaffleName = "Test", Description = "Desc", Id = raffleId });
        _participantRepoMock.Setup(r => r.GetByIdAsync(participantId)).ReturnsAsync((Participant?)null);

        // Act
        var result = await _sut.DeleteParticipant(participantId, raffleId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Participante não encontrada.");
    }
}
