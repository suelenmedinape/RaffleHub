using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using RaffleHub.Api.Services;
using Xunit;

namespace RaffleHub.Tests.Unit;

public class SupabaseServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly SupabaseService _sut;

    public SupabaseServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Supabase:Url"]).Returns("https://test.supabase.co");
        _configurationMock.Setup(c => c["Supabase:Key"]).Returns("test-key");
        
        _sut = new SupabaseService(_configurationMock.Object);
    }

    [Fact]
    public async Task CreateImage_WhenFileIsNull_ShouldReturnFail()
    {
        // Act
        var result = await _sut.CreateImage(null!, "folder");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Arquivo vazio ou inválido");
    }

    [Fact]
    public async Task CreateImage_WhenExtensionNotAllowed_ShouldReturnFail()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.txt");
        fileMock.Setup(f => f.Length).Returns(100);

        // Act
        var result = await _sut.CreateImage(fileMock.Object, "folder");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Tipo de arquivo não suportado"));
    }

    [Fact]
    public async Task CreateImage_WhenFileTooLarge_ShouldReturnFail()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.png");
        fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB

        // Act
        var result = await _sut.CreateImage(fileMock.Object, "folder");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Arquivo muito grande. Máximo 5MB");
    }
}
