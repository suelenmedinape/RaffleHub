using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.DTOs.Gallery;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Services;
using RaffleHub.Api.Utils.Extensions;

namespace RaffleHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class GalleryController : ControllerBase
{
    private readonly GalleryService _service;

    public GalleryController(GalleryService service)
    {
        _service = service;
    }
    
    [HttpGet]
    public async Task<IActionResult> ListAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50) => 
        (await _service.ListAll(page, pageSize)).ToResponse();

    [HttpGet("by-years")]
    public async Task<IActionResult> ListByYears([FromQuery] int[] years, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) => 
        (await _service.ListByYears(years, page, pageSize)).ToResponse();
    
    [HttpGet("by-category/{categoryId:guid}")]
    public async Task<IActionResult> ListByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) => 
        (await _service.ListByCategory(categoryId, page, pageSize)).ToResponse();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => 
        (await _service.GetById(id)).ToResponse();
    
    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpPost]
    public async Task<IActionResult> NewGallery(IFormFile? file, [FromForm] CreateGalleryDto dto)
    {
        var result = await _service.CreateGallery(file, dto);
        return result.ToCreatedResponse(nameof(GetById), new { id = result.Value }, "Imagem criada com sucesso!");
    }
    
    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGallery(Guid id, IFormFile? file, [FromForm] UpdateGalleryDto dto) =>
        (await _service.UpdateGallery(id, file, dto)).ToResponse("Imagem atualizada com sucesso!");
    
    [Authorize(Roles = "ADMIN,OPERATOR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGallery(Guid id) =>
        (await _service.DeleteGallery(id)).ToResponse("Imagem deletada com sucesso!");
}