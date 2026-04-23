namespace RaffleHub.Api.DTOs.Gallery;

public class ListAllGalleryDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    public string NameImage { get; set; }
    public string DescriptionImage { get; set; }
    public int Year { get; set; }
    public Guid CategoryId { get; set; }
}