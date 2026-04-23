using System.ComponentModel.DataAnnotations;
using Hangfire.PostgreSql.Properties;

namespace RaffleHub.Api.DTOs.Gallery;

    public class CreateGalleryDto
{
    public string? ImageUrl { get; set; }
    
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