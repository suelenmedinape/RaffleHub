using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using FluentResults;
using RaffleHub.Api.Services;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Services.Interface;
using Microsoft.AspNetCore.Http;

namespace RaffleHub.Tests;

public class RaffleServiceTests
{
    private readonly Mock<IRaffleRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISupabaseService> _supabaseMock;
    private readonly RaffleService _sut;

    public RaffleServiceTests()
    {
        _repositoryMock = new Mock<IRaffleRepository>();
        _mapperMock = new Mock<IMapper>();
        _supabaseMock = new Mock<ISupabaseService>();
        
        var config = new MapperConfiguration(cfg => {});
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(config);

        _sut = new RaffleService(_repositoryMock.Object, _mapperMock.Object, _supabaseMock.Object);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk_WithListOfRaffles()
    {
        // Arrange
        var expectedRaffles = new List<ListAllRaffleDto> 
        { 
            new() { RaffleName = "Rifa 1" }, 
            new() { RaffleName = "Rifa 2" } 
        };
        _repositoryMock.Setup(r => r.ListAll<ListAllRaffleDto>(It.IsAny<IConfigurationProvider>()))
            .ReturnsAsync(expectedRaffles);

        // Act
        var result = await _sut.ListAll();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedRaffles);
    }

    [Fact]
    public async Task ListNamesRaffle_ShouldReturnOk_WithListOfNames()
    {
        // Arrange
        var expectedNames = new List<ListNamesRafflesDto> 
        { 
            new() { RaffleName = "Rifa 1" }, 
            new() { RaffleName = "Rifa 2" } 
        };
        _repositoryMock.Setup(r => r.ListAll<ListNamesRafflesDto>(It.IsAny<IConfigurationProvider>()))
            .ReturnsAsync(expectedNames);

        // Act
        var result = await _sut.ListNamesRaffle();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnOk_WithDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Id = id };
        var dto = new ListAllRaffleDto { ImageUrl = "I", RaffleName = "Rifa Teste" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(raffle);
        _mapperMock.Setup(m => m.Map<ListAllRaffleDto>(raffle)).Returns(dto);

        // Act
        var result = await _sut.GetById(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Raffle?)null);

        // Act
        var result = await _sut.GetById(id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa não encontrada.");
    }

    [Fact]
    public async Task CreateRaffle_ShouldMapAddSave_AndReturnOkWithId_WhenFileIsNull()
    {
        // Arrange
        var dto = new CreateRaffleDto 
        { RaffleName = "Rifa Teste", Description = "Descrição Teste", TotalTickets = 100, TicketPrice = 10.0m, DrawDate = DateTime.UtcNow.AddDays(7) };
        var raffleId = Guid.NewGuid();
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Id = raffleId };

        _mapperMock.Setup(m => m.Map<Raffle>(dto)).Returns(raffle);

        // Act
        var result = await _sut.CreateRaffle(null, dto);

        // Assert
        _repositoryMock.Verify(r => r.Add(raffle), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _supabaseMock.Verify(s => s.CreateImage(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(raffleId);
    }

    [Fact]
    public async Task CreateRaffle_ShouldUploadImage_WhenFileIsProvided()
    {
        // Arrange
        var dto = new CreateRaffleDto 
        { RaffleName = "Rifa com Foto", Description = "Descrição Teste", TotalTickets = 100, TicketPrice = 10.0m, DrawDate = DateTime.UtcNow.AddDays(7) };
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Id = Guid.NewGuid() };
        var fileMock = new Mock<IFormFile>();
        var imageUrl = "http://supabase.com/image.png";

        _mapperMock.Setup(m => m.Map<Raffle>(dto)).Returns(raffle);
        _supabaseMock.Setup(s => s.CreateImage(fileMock.Object, It.IsAny<string>()))
            .ReturnsAsync(Result.Ok(imageUrl));

        // Act
        var result = await _sut.CreateRaffle(fileMock.Object, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        raffle.ImageUrl.Should().Be(imageUrl);
        _repositoryMock.Verify(r => r.Add(raffle), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRaffle_ShouldReturnFail_WhenImageUploadFails()
    {
        // Arrange
        var dto = new CreateRaffleDto 
        { RaffleName = "Rifa com Foto", Description = "Descrição Teste", TotalTickets = 100, TicketPrice = 10.0m, DrawDate = DateTime.UtcNow.AddDays(7) };
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Id = Guid.NewGuid() };
        var fileMock = new Mock<IFormFile>();

        _mapperMock.Setup(m => m.Map<Raffle>(dto)).Returns(raffle);
        _supabaseMock.Setup(s => s.CreateImage(fileMock.Object, It.IsAny<string>()))
            .ReturnsAsync(Result.Fail("Erro no upload"));

        // Act
        var result = await _sut.CreateRaffle(fileMock.Object, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Erro no upload");
        _repositoryMock.Verify(r => r.Add(It.IsAny<Raffle>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRaffle_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateRaffleDto { RaffleName = "Nome", Description = "Desc" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Raffle?)null);

        // Act
        var result = await _sut.UpdateRaffle(id, null, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa não encontrada.");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Raffle>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRaffle_WhenTotalTicketsIsReduced_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var currentRaffle = new Raffle { RaffleName = "Test", Description = "Desc", TotalTickets = 100 };
        var dto = new UpdateRaffleDto 
        { RaffleName = "Nome", Description = "Desc", TotalTickets = 50 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(currentRaffle);

        // Act
        var result = await _sut.UpdateRaffle(id, null, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Não é possível reduzir o número total de bilhetes"));
        _repositoryMock.Verify(r => r.Update(It.IsAny<Raffle>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRaffle_WhenValid_ShouldMapUpdateSave_AndReturnOkWithId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var currentRaffle = new Raffle { RaffleName = "Test", Description = "Desc", Id = id, TotalTickets = 100 };
        var dto = new UpdateRaffleDto 
        { RaffleName = "Nome Atualizado", Description = "Desc Atualizada", TotalTickets = 150 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(currentRaffle);

        // Act
        var result = await _sut.UpdateRaffle(id, null, dto);

        // Assert
        _mapperMock.Verify(m => m.Map(dto, currentRaffle), Times.Once);
        _repositoryMock.Verify(r => r.Update(currentRaffle), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
    }

    [Fact]
    public async Task ChangeRaffleStatus_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStatusDto { Status = RaffleStatus.ACTIVE };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Raffle?)null);

        // Act
        var result = await _sut.ChangeRaffleStatus(id, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa não encontrada.");
    }

    [Fact]
    public async Task ChangeRaffleStatus_WhenExpiredAndDrawDateInPast_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStatusDto { Status = RaffleStatus.ACTIVE };
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Status = RaffleStatus.EXPIRED, DrawDate = DateTime.UtcNow.AddDays(-1) };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(raffle);

        // Act
        var result = await _sut.ChangeRaffleStatus(id, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa expirada. Edite a data do sorteio antes de reativá-la.");
    }

    [Fact]
    public async Task ChangeRaffleStatus_WhenCancelled_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStatusDto { Status = RaffleStatus.ACTIVE };
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Status = RaffleStatus.CANCELLED };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(raffle);

        // Act
        var result = await _sut.ChangeRaffleStatus(id, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa cancelada não pode ter o status alterado.");
    }

    [Fact]
    public async Task ChangeRaffleStatus_WhenCompleted_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStatusDto { Status = RaffleStatus.ACTIVE };
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Status = RaffleStatus.COMPLETED };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(raffle);

        // Act
        var result = await _sut.ChangeRaffleStatus(id, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa já finalizada não pode ter o status alterado.");
    }

    [Fact]
    public async Task ChangeRaffleStatus_WhenStatusIsSame_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStatusDto { Status = RaffleStatus.ACTIVE };
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Status = RaffleStatus.ACTIVE, DrawDate = DateTime.UtcNow.AddDays(1) };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(raffle);

        // Act
        var result = await _sut.ChangeRaffleStatus(id, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == $"A rifa já está com o status '{dto.Status}'.");
    }

    [Fact]
    public async Task ChangeRaffleStatus_WhenExpiredAndDrawDateInPast_ShouldReturnFailWithSpecificMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStatusDto { Status = RaffleStatus.ACTIVE };
        var raffle = new Raffle 
        { Status = RaffleStatus.EXPIRED, DrawDate = DateTime.UtcNow.AddDays(-1) };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(raffle);

        // Act
        var result = await _sut.ChangeRaffleStatus(id, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Rifa expirada. Edite a data do sorteio antes de reativá-la.");
    }

    [Fact]
    public async Task ChangeRaffleStatus_WhenValid_ShouldUpdateSave_AndReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStatusDto { Status = RaffleStatus.ACTIVE };
        var raffle = new Raffle { RaffleName = "Test", Description = "Desc", Status = RaffleStatus.EXPIRED, DrawDate = DateTime.UtcNow.AddDays(1) };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(raffle);

        // Act
        var result = await _sut.ChangeRaffleStatus(id, dto);

        // Assert
        raffle.Status.Should().Be(RaffleStatus.ACTIVE);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be($"Rifa {dto.Status} com sucesso");
    }
}
