using AutoMapper;
using FluentAssertions;
using Moq;
using RaffleHub.Api.DTOs.CategoriesGallery;
using RaffleHub.Api.Entities;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Services;
using FluentResults;

namespace RaffleHub.Tests.Unit;

public class CategoriesGalleryServiceTests
{
    private readonly Mock<ICategoriesGalleryRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CategoriesGalleryService _sut;

    public CategoriesGalleryServiceTests()
    {
        _repositoryMock = new Mock<ICategoriesGalleryRepository>();
        _mapperMock = new Mock<IMapper>();
        _sut = new CategoriesGalleryService(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk_WithCategories()
    {
        // Arrange
        var categories = new List<CategoriesGallery> { new() { Id = Guid.NewGuid(), CategoryName = "Cat1" } };
        var dtos = new List<ListAllCategoriesDto> { new() { Id = categories[0].Id, CategoryName = "Cat1" } };
        
        _repositoryMock.Setup(r => r.ListAll()).ReturnsAsync(categories);
        _mapperMock.Setup(m => m.Map<List<ListAllCategoriesDto>>(categories)).Returns(dtos);

        // Act
        var result = await _sut.ListAll();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new CategoriesGallery { CategoryName = "Cat", Id = id };
        var dto = new ListAllCategoriesDto { Id = id, CategoryName = "Cat1" };
        
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(category);
        _mapperMock.Setup(m => m.Map<ListAllCategoriesDto>(category)).Returns(dto);

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
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync((CategoriesGallery?)null);

        // Act
        var result = await _sut.GetById(id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Categoria não encontrada.");
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnOk_AndCallSave()
    {
        // Arrange
        var dto = new CreateCategoriesGalleryDto { CategoryName = "New Cat" };
        var category = new CategoriesGallery { CategoryName = "Cat", Id = Guid.NewGuid() };
        
        _mapperMock.Setup(m => m.Map<CategoriesGallery>(dto)).Returns(category);

        // Act
        var result = await _sut.CreateCategory(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(category.Id);
        _repositoryMock.Verify(r => r.Add(category), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCategoriesGalleryDto { CategoryName = "Updated Cat" };
        var category = new CategoriesGallery { CategoryName = "Cat", Id = id };
        
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(category);

        // Act
        var result = await _sut.UpdateCategory(id, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
        _mapperMock.Verify(m => m.Map(dto, category), Times.Once);
        _repositoryMock.Verify(r => r.Update(category), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCategoriesGalleryDto { CategoryName = "Updated Cat" };
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync((CategoriesGallery?)null);

        // Act
        var result = await _sut.UpdateCategory(id, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Categoria não encontrada.");
    }

    [Fact]
    public async Task DeleteCategory_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new CategoriesGallery { CategoryName = "Cat", Id = id };
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(category);

        // Act
        var result = await _sut.DeleteCategory(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(category), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_WhenNotExists_ShouldReturnFail()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync((CategoriesGallery?)null);

        // Act
        var result = await _sut.DeleteCategory(id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Categoria não encontrada.");
    }
}
