using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RaffleHub.Api.Services;
using Xunit;

namespace RaffleHub.Tests.Unit;

public class N8nServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<N8nService>> _loggerMock;
    private readonly N8nService _sut;

    public N8nServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<N8nService>>();

        _configurationMock.Setup(c => c["N8n:WebhookUrl"]).Returns("http://n8n.test/webhook");

        _sut = new N8nService(httpClient, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendWhatsAppNotificationAsync_WhenUrlNotConfigured_ShouldLogWarning()
    {
        // Arrange
        _configurationMock.Setup(c => c["N8n:WebhookUrl"]).Returns((string?)null);

        // Act
        await _sut.SendWhatsAppNotificationAsync("123", "Test", new List<int> { 1 }, "Rifa");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("n8n Webhook URL is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWhatsAppNotificationAsync_WhenRequestFails_ShouldLogError()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("Error message") });

        // Act
        await _sut.SendWhatsAppNotificationAsync("123", "Test", new List<int> { 1 }, "Rifa");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send notification to n8n")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWhatsAppNotificationAsync_WhenRequestSucceeds_ShouldLogInformation()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            { StatusCode = HttpStatusCode.OK });

        // Act
        await _sut.SendWhatsAppNotificationAsync("123", "Test", new List<int> { 1 }, "Rifa");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully sent WhatsApp notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
