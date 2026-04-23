using FluentResults;

namespace RaffleHub.Api.Services.Interface;

public interface ISupabaseService
{
    Task<Result<string>> CreateImage(IFormFile file, string folderName);
    Task<Result<bool>> DeleteImageAsync(string imageUrl);
}