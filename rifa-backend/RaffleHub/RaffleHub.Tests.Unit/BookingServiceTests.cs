using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RaffleHub.Api.DTOs.Booking;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services;
using RaffleHub.Api.Services.MercadoPago;
using RaffleHub.Api.Services.Interface;
using FluentResults;
using MockQueryable.Moq;
using MockQueryable;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using RaffleHub.Api.Hubs;
using RaffleHub.Api.Features.Payments.ConfirmPayment;

namespace RaffleHub.Tests.Unit;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _repositoryMock;
    private readonly Mock<IParticipantRepository> _participantRepositoryMock;
    private readonly Mock<IMercadoPagoService> _mercadoPagoServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobsMock;
    private readonly Mock<ILogger<BookingService>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IHubContext<PaymentNotificationHub>> _hubContextMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _repositoryMock = new Mock<IBookingRepository>();
        _participantRepositoryMock = new Mock<IParticipantRepository>();
        _mercadoPagoServiceMock = new Mock<IMercadoPagoService>();
        _mapperMock = new Mock<IMapper>();
        _backgroundJobsMock = new Mock<IBackgroundJobClient>();
        _loggerMock = new Mock<ILogger<BookingService>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _hubContextMock = new Mock<IHubContext<PaymentNotificationHub>>();
        _mediatorMock = new Mock<IMediator>();

        _sut = new BookingService(
            _repositoryMock.Object,
            _participantRepositoryMock.Object,
            _mercadoPagoServiceMock.Object,
            _mapperMock.Object,
            _backgroundJobsMock.Object,
            _loggerMock.Object,
            _httpContextAccessorMock.Object,
            _hubContextMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task GetMyBookingsAsync_WhenNotAuthenticated_ShouldReturnFail()
    {
        // Arrange
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = await _sut.GetMyBookingsAsync();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Usuário não autenticado");
    }

    [Fact]
    public async Task GetMyBookingsAsync_WhenAuthenticated_ShouldReturnOk()
    {
        // Arrange
        var userId = "user123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

        var participant = new Participant { ParticipantName = "Test", Phone = "123", Cpf = "123", Id = Guid.NewGuid(), UserId = userId };
        var bookings = new List<Booking>
        {
            new Booking 
            { Id = Guid.NewGuid(), ParticipantId = participant.Id, Participant = participant, Raffle = new Raffle { RaffleName = "Rifa1" },
                Tickets = new List<Ticket> { new Ticket { Raffle = null!, TicketNumber = 1 } }
            }
        }.BuildMock();

        _repositoryMock.Setup(r => r.GetQueryable()).Returns(bookings);

        // Act
        var result = await _sut.GetMyBookingsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().RaffleName.Should().Be("Rifa1");
    }

    [Fact]
    public async Task BookingPending_WhenParticipantNotFound_ShouldReturnFail()
    {
        // Arrange
        var participantId = Guid.NewGuid();
        _participantRepositoryMock.Setup(p => p.GetByIdAsync(participantId)).ReturnsAsync((Participant?)null);

        // Act
        var result = await _sut.BookingPending(participantId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Esse participante não existe");
    }

    [Fact]
    public async Task BookingPending_WhenNoPendingBooking_ShouldReturnFail()
    {
        // Arrange
        var participantId = Guid.NewGuid();
        _participantRepositoryMock.Setup(p => p.GetByIdAsync(participantId)).ReturnsAsync(new Participant { ParticipantName = "Test", Phone = "123", Cpf = "123", Id = participantId });
        
        var bookings = new List<Booking>().BuildMock();
        _repositoryMock.Setup(r => r.GetQueryable()).Returns(bookings);

        // Act
        var result = await _sut.BookingPending(participantId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Nenhuma reserva encontrada para esse participante.");
    }

    [Fact]
    public async Task GeneratePixPaymentAsync_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var participantId = Guid.NewGuid();
        var participant = new Participant { ParticipantName = "Test", Phone = "123", Cpf = "123", Id = participantId };
        var booking = new Booking { Participant = null!, Raffle = null!, Id = Guid.NewGuid(), ParticipantId = participantId, Status = BookingStatus.PENDING, TotalAmount = 100 };
        
        _participantRepositoryMock.Setup(p => p.GetByIdAsync(participantId)).ReturnsAsync(participant);
        
        var bookings = new List<Booking> { booking }.BuildMock();
        _repositoryMock.Setup(r => r.GetQueryable()).Returns(bookings);

        var pixResponse = new MercadoPagoPixResponseDto { TransactionId = "tx123", PixCopyPaste = "copy-paste" };
        _mercadoPagoServiceMock.Setup(m => m.CreatePixTransactionAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok(pixResponse));

        var dto = new ListBookingPendingDto { Id = booking.Id };
        _mapperMock.Setup(m => m.Map<ListBookingPendingDto>(booking)).Returns(dto);

        // Act
        var result = await _sut.GeneratePixPaymentAsync(participantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.TransactionId.Should().Be("tx123");
        _repositoryMock.Verify(r => r.Update(booking), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfirmPaymentByTransactionIdAsync_ShouldSendConfirmPaymentCommand()
    {
        // Arrange
        var transactionId = "tx123";
        _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmPaymentCommand>(), default))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _sut.ConfirmPaymentByTransactionIdAsync(transactionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mediatorMock.Verify(m => m.Send(
            It.Is<ConfirmPaymentCommand>(c => c.TransactionId == transactionId), 
            default), 
            Times.Once);
    }
}
