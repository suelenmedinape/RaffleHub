using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RaffleHub.Api.DTOs.Gallery;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services;
using RaffleHub.Api.Services.Interface;
using FluentResults;

namespace RaffleHub.Tests.Unit;

public class GalleryServiceTests
{
    private readonly Mock<IGalleryRepository> _repositoryMock;
    private readonly Mock<ISupabaseService> _supabaseMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GalleryService _sut;

    public GalleryServiceTests()
    {
        _repositoryMock = new Mock<IGalleryRepository>();
        _supabaseMock = new Mock<ISupabaseService>();
        _mapperMock = new Mock<IMapper>();
        _sut = new GalleryService(_repositoryMock.Object, _mapperMock.Object, _supabaseMock.Object);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk_WithPaginatedData()
    {
        // Arrange
        var expectedResult = new List<ListAllGalleryDto> { new() { Id = Guid.NewGuid(), NameImage = "Img1" } };
        _repositoryMock.Setup(r => r.FindAllPaginated<ListAllGalleryDto>(It.IsAny<IConfigurationProvider>(), 1, 10))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ListAll(1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var gallery = new Gallery { ImageUrl = "I", NameImage = "N", CategoriesGallery = null!, Id = id };
        var dto = new ListAllGalleryDto { ImageUrl = "I", DescriptionImage = "D", Id = id, NameImage = "Img1" };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(gallery);
        _mapperMock.Setup(m => m.Map<ListAllGalleryDto>(gallery)).Returns(dto);

        // Act
        var result = await _sut.GetById(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetById_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Gallery?)null);

        // Act
        var result = await _sut.GetById(id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Imagem não encontrada.");
    }

    [Fact]
    public async Task CreateGallery_WithFile_ShouldUploadAndReturnOk()
    {
        // Arrange
        var dto = new CreateGalleryDto { NameImage = "New Img", CategoryId = Guid.NewGuid() };
        var gallery = new Gallery { ImageUrl = "I", NameImage = "N", CategoriesGallery = null!, Id = Guid.NewGuid(), FolderName = "gallery" };
        var fileMock = new Mock<IFormFile>();
        var imageUrl = "http://supabase.com/img.png";

        _mapperMock.Setup(m => m.Map<Gallery>(dto)).Returns(gallery);
        _supabaseMock.Setup(s => s.CreateImage(fileMock.Object, gallery.FolderName))
            .ReturnsAsync(Result.Ok(imageUrl));

        // Act
        var result = await _sut.CreateGallery(fileMock.Object, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(gallery.Id);
        gallery.ImageUrl.Should().Be(imageUrl);
        _repositoryMock.Verify(r => r.Add(gallery), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateGallery_WithoutFile_ShouldReturnOk()
    {
        // Arrange
        var dto = new CreateGalleryDto { NameImage = "New Img", ImageUrl = "existing.png" };
        var gallery = new Gallery { ImageUrl = "I", NameImage = "N", CategoriesGallery = null!, Id = Guid.NewGuid() };

        _mapperMock.Setup(m => m.Map<Gallery>(dto)).Returns(gallery);

        // Act
        var result = await _sut.CreateGallery(null, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(gallery.Id);
        gallery.ImageUrl.Should().Be("existing.png");
        _repositoryMock.Verify(r => r.Add(gallery), Times.Once);
    }

    [Fact]
    public async Task UpdateGallery_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateGalleryDto { NameImage = "Updated Img" };
        var gallery = new Gallery { ImageUrl = "I", NameImage = "N", CategoriesGallery = null!, Id = id };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(gallery);

        // Act
        var result = await _sut.UpdateGallery(id, null, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
        _mapperMock.Verify(m => m.Map(dto, gallery), Times.Once);
        _repositoryMock.Verify(r => r.Update(gallery), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateGallery_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateGalleryDto { NameImage = "Updated Img" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Gallery?)null);

        // Act
        var result = await _sut.UpdateGallery(id, null, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Dados não encontrados");
    }

    [Fact]
    public async Task DeleteGallery_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var gallery = new Gallery { ImageUrl = "I", NameImage = "N", CategoriesGallery = null!, Id = id };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(gallery);

        // Act
        var result = await _sut.DeleteGallery(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(gallery), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGallery_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Gallery?)null);

        // Act
        var result = await _sut.DeleteGallery(id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Dados não encontrados");
    }
}
