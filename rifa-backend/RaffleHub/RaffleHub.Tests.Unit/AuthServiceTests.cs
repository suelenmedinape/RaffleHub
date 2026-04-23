using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using RaffleHub.Api.DTOs.Auth;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Services;
using Xunit;

namespace RaffleHub.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<RaffleHub.Api.Repositories.Interfaces.IParticipantRepository> _participantRepoMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        
        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);
        
        _configurationMock = new Mock<IConfiguration>();
        _participantRepoMock = new Mock<RaffleHub.Api.Repositories.Interfaces.IParticipantRepository>();
        
        // Setup configuration
        _configurationMock.Setup(c => c["JWT:SecretKey"]).Returns("a_very_long_secret_key_with_at_least_32_characters");
        _configurationMock.Setup(c => c["JWT:ValidIssuer"]).Returns("issuer");
        _configurationMock.Setup(c => c["JWT:ValidAudience"]).Returns("audience");

        _sut = new AuthService(_userManagerMock.Object, _roleManagerMock.Object, _configurationMock.Object, _participantRepoMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenEmailExists_ShouldReturnFail()
    {
        // Arrange
        var dto = new RegisterDto { Email = "test@test.com" };
        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(new ApplicationUser());

        // Act
        var result = await _sut.RegisterUserAsync(dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Já existe um usuário com esse e-mail.");
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidCredentials_ShouldReturnFail()
    {
        // Arrange
        var dto = new LoginDto { Email = "test@test.com", Password = "wrong" };
        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(new ApplicationUser());
        _userManagerMock.Setup(u => u.CheckPasswordAsync(It.IsAny<ApplicationUser>(), dto.Password)).ReturnsAsync(false);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Usuário não encontrado ou senha inválida.");
    }

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = new ApplicationUser { Id = "1", Email = "test@test.com", FullName = "Test User" };
        var dto = new LoginDto { Email = "test@test.com", Password = "password" };
        
        _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "ADMIN" });
        _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Phone.Should().Be(user.PhoneNumber);
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RevokeAsync_WhenUserExists_ShouldClearRefreshToken()
    {
        // Arrange
        var email = "test@test.com";
        var user = new ApplicationUser { Email = email, RefreshToken = "old-token" };
        _userManagerMock.Setup(u => u.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.RevokeAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.RefreshToken.Should().BeNull();
        _userManagerMock.Verify(u => u.UpdateAsync(user), Times.Once);
    }
}
