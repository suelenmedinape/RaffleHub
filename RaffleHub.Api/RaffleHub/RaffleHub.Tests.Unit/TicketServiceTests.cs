using AutoMapper;
using FluentAssertions;
using Moq;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services;
using MockQueryable.Moq;
using MockQueryable;
using FluentResults;

namespace RaffleHub.Tests.Unit;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _repositoryMock;
    private readonly Mock<IRaffleRepository> _raffleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TicketService _sut;

    public TicketServiceTests()
    {
        _repositoryMock = new Mock<ITicketRepository>();
        _raffleRepositoryMock = new Mock<IRaffleRepository>();
        _mapperMock = new Mock<IMapper>();
        _sut = new TicketService(_repositoryMock.Object, _raffleRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task ListTicketsSold_WhenRaffleNotFound_ShouldReturnFail()
    {
        // Arrange
        var raffleId = Guid.NewGuid();
        _raffleRepositoryMock.Setup(r => r.GetByIdAsync(raffleId)).ReturnsAsync((Raffle?)null);

        // Act
        var result = await _sut.ListTicketsSold(raffleId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa não encontrada");
    }

    [Fact]
    public async Task ListTicketsSold_WhenRaffleExists_ShouldReturnSoldTicketNumbers()
    {
        // Arrange
        var raffleId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        _raffleRepositoryMock.Setup(r => r.GetByIdAsync(raffleId)).ReturnsAsync(new Raffle { RaffleName = "Test", Description = "Desc", Id = raffleId });

        var tickets = new List<Ticket>
        {
            new Ticket { Raffle = null!, RaffleId = raffleId, TicketNumber = 10, ParticipantId = participantId },
            new Ticket { Raffle = null!, RaffleId = raffleId, TicketNumber = 5, ParticipantId = participantId },
            new Ticket { Raffle = null!, RaffleId = raffleId, TicketNumber = 20, ParticipantId = null }, // Not sold
            new Ticket { Raffle = null!, RaffleId = Guid.NewGuid(), TicketNumber = 1, ParticipantId = participantId } // Different raffle
        }.BuildMock();

        _repositoryMock.Setup(r => r.GetQueryable()).Returns(tickets);

        // Act
        var result = await _sut.ListTicketsSold(raffleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeInAscendingOrder();
        result.Value.Should().Equal(5, 10);
    }
}
