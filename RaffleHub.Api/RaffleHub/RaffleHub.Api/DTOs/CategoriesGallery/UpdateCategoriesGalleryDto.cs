using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RaffleHub.Api.DTOs.CategoriesGallery;

public class UpdateCategoriesGalleryDto
{
    [NotNull]
    [Required(ErrorMessage = "O Campo nome não pode estar em branco")]
    public string CategoryName { get; set; }
}