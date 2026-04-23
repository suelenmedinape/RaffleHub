using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RaffleHub.Api.DTOs.Gallery;

public class UpdateGalleryDto
{
    
    [NotNull]
    [Required(ErrorMessage = "O campo nome da imagem nao pode estar vazio.")]
    public string NameImage { get; set; }
    
    public string? DescriptionImage { get; set; }
    
    [NotNull]
    [Required(ErrorMessage = "O campo ano do evento não pode estar vazio")]
    [Range(1900, 2100, ErrorMessage = "Ano inválido")]
    public int Year { get; set; }
    
    [NotNull]
    public Guid CategoryId { get; set; }
}