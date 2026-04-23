using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.Gallery;
using RaffleHub.Api.Utils.ExceptionHandler;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaffleHub.Api.Services;
using RaffleHub.Api.Repositories;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Entities;
using Moq;
using Microsoft.AspNetCore.Http;
using RaffleHub.Api.Services.Interface;
using FluentResults;

namespace RaffleHub.Tests.E2E;

public class GalleryE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly string DbName = Guid.NewGuid().ToString();
    private readonly Mock<ISupabaseService> _supabaseMock = new();

    public GalleryE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                var descriptors = services.Where(
                    d => d.ServiceType.Name.Contains("DbContextOptions") || 
                         d.ServiceType == typeof(AppDbContext) ||
                         d.ServiceType.Name.Contains("IDatabaseProvider")).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DbName);
                });

                services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = "TestAuth";
                    opts.DefaultChallengeScheme = "TestAuth";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });

                services.RemoveAll<ISupabaseService>();
                _supabaseMock.Setup(s => s.CreateImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
                    .ReturnsAsync(Result.Ok("http://supabase.com/test.png"));
                services.AddSingleton<ISupabaseService>(_supabaseMock.Object);

                services.TryAddScoped<IGalleryRepository, GalleryRepository>();
                services.TryAddScoped<ICategoriesGalleryRepository, CategoriesGalleryRepository>();
                services.TryAddScoped<GalleryService>();
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");
    }

    private async Task<(Guid categoryId, Guid galleryId)> SeedData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var category = new CategoriesGallery { CategoryName = "Cat", Id = Guid.NewGuid() };
        context.CategoriesGallery.Add(category);

        var gallery = new Gallery
        { Id = Guid.NewGuid(), NameImage = "Img1", ImageUrl = "test.png", Year = DateTime.UtcNow.Year, CategoryId = category.Id };
        context.Gallery.Add(gallery);
        
        await context.SaveChangesAsync();
        return (category.Id, gallery.Id);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk()
    {
        // Arrange
        await SeedData();

        // Act
        var response = await _client.GetAsync("/Gallery");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ListAllGalleryDto>>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data!);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var ids = await SeedData();

        // Act
        var response = await _client.GetAsync($"/Gallery/{ids.galleryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ListAllGalleryDto>>();
        Assert.NotNull(result);
        Assert.Equal(ids.galleryId, result.Data!.Id);
    }

    [Fact]
    public async Task CreateGallery_ShouldReturnCreated()
    {
        // Arrange
        var ids = await SeedData();
        var dto = new CreateGalleryDto 
        { NameImage = "New E2E Img", CategoryId = ids.categoryId, Year = DateTime.UtcNow.Year, ImageUrl = "placeholder.png" };
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.NameImage), nameof(dto.NameImage));
        content.Add(new StringContent(dto.CategoryId.ToString()), nameof(dto.CategoryId));
        content.Add(new StringContent(dto.Year.ToString("o")), nameof(dto.Year));
        content.Add(new StringContent(dto.ImageUrl), nameof(dto.ImageUrl));

        // Act
        var response = await _client.PostAsync("/Gallery", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Data);
    }

    [Fact]
    public async Task UpdateGallery_ShouldReturnOk()
    {
        // Arrange
        var ids = await SeedData();
        var dto = new UpdateGalleryDto 
        { NameImage = "Updated Img", CategoryId = ids.categoryId, Year = DateTime.UtcNow.Year };
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.NameImage), nameof(dto.NameImage));
        content.Add(new StringContent(dto.CategoryId.ToString()), nameof(dto.CategoryId));
        content.Add(new StringContent(dto.Year.ToString()), nameof(dto.Year));

        // Act
        var response = await _client.PutAsync($"/Gallery/{ids.galleryId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.Equal(ids.galleryId, result!.Data);
    }

    [Fact]
    public async Task DeleteGallery_ShouldReturnOk()
    {
        // Arrange
        var ids = await SeedData();

        // Act
        var response = await _client.DeleteAsync($"/Gallery/{ids.galleryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal("Imagem deletada com sucesso!", result!.Message);
    }
}
