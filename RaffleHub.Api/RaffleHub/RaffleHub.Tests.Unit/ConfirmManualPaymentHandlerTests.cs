using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Features.Payments.ConfirmManualPayment;
using RaffleHub.Api.Hubs;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Entities;
using MockQueryable.Moq;
using MockQueryable;
using Microsoft.EntityFrameworkCore;

namespace RaffleHub.Tests.Unit;

public class ConfirmManualPaymentHandlerTests
{
    private readonly Mock<IBookingRepository> _repositoryMock;
    private readonly Mock<ILogger<ConfirmManualPaymentHandler>> _loggerMock;
    private readonly Mock<IHubContext<PaymentNotificationHub>> _hubContextMock;
    private readonly ConfirmManualPaymentHandler _sut;

    public ConfirmManualPaymentHandlerTests()
    {
        _repositoryMock = new Mock<IBookingRepository>();
        _loggerMock = new Mock<ILogger<ConfirmManualPaymentHandler>>();
        _hubContextMock = new Mock<IHubContext<PaymentNotificationHub>>();

        // Setup Hub Mocks
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

        _sut = new ConfirmManualPaymentHandler(
            _repositoryMock.Object,
            _loggerMock.Object,
            _hubContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBookingNotFound_ShouldReturnFail()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var mock = new List<Booking>().BuildMock();
        _repositoryMock.Setup(r => r.GetQueryable()).Returns(mock);

        var command = new ConfirmManualPaymentCommand(bookingId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Reserva não encontrada.");
        _repositoryMock.Verify(r => r.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyPaid_ShouldReturnFail()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking { Id = bookingId, Status = BookingStatus.PAID };
        var mock = new List<Booking> { booking }.BuildMock();
        _repositoryMock.Setup(r => r.GetQueryable()).Returns(mock);

        var command = new ConfirmManualPaymentCommand(bookingId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Esta reserva já está paga.");
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldUpdateStatusAndNotify()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking 
        { 
            Id = bookingId, 
            Status = BookingStatus.PENDING,
            Raffle = new Raffle { RaffleName = "Mega Rifa" },
            Tickets = new List<Ticket> { new Ticket { TicketNumber = 42 } }
        };
        var mock = new List<Booking> { booking }.BuildMock();
        _repositoryMock.Setup(r => r.GetQueryable()).Returns(mock);

        var command = new ConfirmManualPaymentCommand(bookingId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.PAID);
        booking.PaidAt.Should().NotBeNull();
        
        _repositoryMock.Verify(r => r.Update(booking), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _repositoryMock.Verify(r => r.CommitTransactionAsync(), Times.Once);
        
        // SignalR verification
        _hubContextMock.Verify(h => h.Clients.Group(bookingId.ToString()), Times.Once);
    }
}
