using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.DTOs.CategoriesGallery;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Services;
using RaffleHub.Api.Utils.Extensions;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CategoriesGalleryController : ControllerBase
{
    private readonly CategoriesGalleryService _service;

    public CategoriesGalleryController(CategoriesGalleryService service)
    {
        _service = service;
    }
    
    [HttpGet]
    public async Task<IActionResult> ListAll() => (await _service.ListAll()).ToResponse();
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) => 
        (await _service.GetById(id)).ToResponse();
    
    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpPost]
    public async Task<IActionResult> NewCategory([FromBody] CreateCategoriesGalleryDto dto)
    {
        var result = await _service.CreateCategory(dto);
        return result.ToCreatedResponse(nameof(GetById), new { id = result.Value }, "Categoria criada com sucesso!");
    }
    
    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoriesGalleryDto dto) =>
        (await _service.UpdateCategory(id, dto)).ToResponse("Categoria atualizada com sucesso!");
    
    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id) =>
        (await _service.DeleteCategory(id)).ToResponse("Categoria deletada com sucesso!");
}