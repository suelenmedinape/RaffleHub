namespace RaffleHub.Api.Entities;

public class Gallery
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    public string FolderName { get; set; } = "gallery";
    public string NameImage { get; set; }
    public string? DescriptionImage { get; set; }
    public int Year { get; set; }

    public CategoriesGallery CategoriesGallery { get; set; }
    public Guid CategoryId { get; set; }
}