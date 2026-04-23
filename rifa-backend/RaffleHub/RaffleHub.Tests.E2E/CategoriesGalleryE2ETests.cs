using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.CategoriesGallery;
using RaffleHub.Api.Utils.ExceptionHandler;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaffleHub.Api.Services;
using RaffleHub.Api.Repositories;
using RaffleHub.Api.Repositories.Interfaces;
using RaffleHub.Api.Entities;

namespace RaffleHub.Tests.E2E;

public class CategoriesGalleryE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly string DbName = Guid.NewGuid().ToString();

    public CategoriesGalleryE2ETests(WebApplicationFactory<Program> factory)
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

                services.TryAddScoped<ICategoriesGalleryRepository, CategoriesGalleryRepository>();
                services.TryAddScoped<CategoriesGalleryService>();
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("TestAuth");
    }

    private async Task<Guid> SeedCategory(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = new CategoriesGallery
        { Id = Guid.NewGuid(), CategoryName = name };
        context.CategoriesGallery.Add(category);
        await context.SaveChangesAsync();
        return category.Id;
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk()
    {
        // Arrange
        await SeedCategory("Cat1");

        // Act
        var response = await _client.GetAsync("/CategoriesGallery");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ListAllCategoriesDto>>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data!);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnOk()
    {
        // Arrange
        var id = await SeedCategory("CatGet");

        // Act
        var response = await _client.GetAsync($"/CategoriesGallery/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ListAllCategoriesDto>>();
        Assert.NotNull(result);
        Assert.Equal(id, result.Data!.Id);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated()
    {
        // Arrange
        var dto = new CreateCategoriesGalleryDto { CategoryName = "New E2E Cat" };
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.CategoryName), nameof(dto.CategoryName));

        // Act
        var response = await _client.PostAsync("/CategoriesGallery", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Data);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnOk()
    {
        // Arrange
        var id = await SeedCategory("Old Cat");
        var dto = new UpdateCategoriesGalleryDto { CategoryName = "Updated Cat" };

        // Act
        var response = await _client.PutAsJsonAsync($"/CategoriesGallery/{id}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.Equal(id, result!.Data);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnOk()
    {
        // Arrange
        var id = await SeedCategory("Delete Cat");

        // Act
        var response = await _client.DeleteAsync($"/CategoriesGallery/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal("Categoria deletada com sucesso!", result!.Message);
    }
}
