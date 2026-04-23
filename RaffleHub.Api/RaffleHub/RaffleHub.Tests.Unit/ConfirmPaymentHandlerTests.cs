using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Features.Payments.ConfirmPayment;
using RaffleHub.Api.Hubs;
using RaffleHub.Api.Repositories.Interfaces;

namespace RaffleHub.Tests.Unit;

public class ConfirmPaymentHandlerTests
{
    private readonly Mock<IBookingRepository> _repositoryMock;
    private readonly Mock<ILogger<ConfirmPaymentHandler>> _loggerMock;
    private readonly Mock<IHubContext<PaymentNotificationHub>> _hubContextMock;
    private readonly ConfirmPaymentHandler _sut;

    public ConfirmPaymentHandlerTests()
    {
        _repositoryMock = new Mock<IBookingRepository>();
        _loggerMock = new Mock<ILogger<ConfirmPaymentHandler>>();
        _hubContextMock = new Mock<IHubContext<PaymentNotificationHub>>();

        _sut = new ConfirmPaymentHandler(
            _repositoryMock.Object,
            _loggerMock.Object,
            _hubContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBookingNotFound_ShouldReturnFail()
    {
        // Arrange
        var command = new ConfirmPaymentCommand("tx123");
        _repositoryMock.Setup(r => r.GetByTransactionIdLockedAsync(command.TransactionId))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Reserva não encontrada para essa transação");
        _repositoryMock.Verify(r => r.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBookingFound_ShouldUpdateStatusAndNotifySignalR()
    {
        // Arrange
        var command = new ConfirmPaymentCommand("tx123");
        var booking = new Booking
        { Id = Guid.NewGuid(), TransactionId = "tx123", Status = BookingStatus.PENDING, Raffle = new Raffle { RaffleName = "Rifa Teste" },
            Participant = new Participant { ParticipantName = "Test", Phone = "123", Cpf = "123" },
            Tickets = new List<Ticket> { new Ticket { Raffle = null!, TicketNumber = 1 } }
        };

        _repositoryMock.Setup(r => r.GetByTransactionIdLockedAsync(command.TransactionId))
            .ReturnsAsync(booking);

        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.PAID);
        booking.PaidAt.Should().NotBeNull();
        
        _repositoryMock.Verify(r => r.BeginTransactionAsync(), Times.Once);
        _repositoryMock.Verify(r => r.Update(booking), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repositoryMock.Verify(r => r.CommitTransactionAsync(), Times.Once);

        clientsMock.Verify(c => c.Group(booking.Id.ToString()), Times.Once);
        clientProxyMock.Verify(c => c.SendCoreAsync(
            "PaymentConfirmed", 
            It.Is<object[]>(o => o.Length == 1), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackAndReturnFail()
    {
        // Arrange
        var command = new ConfirmPaymentCommand("tx123");
        _repositoryMock.Setup(r => r.GetByTransactionIdLockedAsync(command.TransactionId))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Erro interno ao confirmar pagamento");
        _repositoryMock.Verify(r => r.RollbackTransactionAsync(), Times.Once);
    }
}
