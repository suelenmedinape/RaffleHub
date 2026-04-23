using System.Text.RegularExpressions;
using FluentResults;
using RaffleHub.Api.Services.Interface;
using Supabase;
using FileOptions = Supabase.Storage.FileOptions;

namespace RaffleHub.Api.Services;

public class SupabaseService : ISupabaseService
{
    private readonly Client _supabase;
    private readonly string _bucketName;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;

    public SupabaseService(IConfiguration configuration)
    {
        _supabaseUrl = configuration["Supabase:Url"] ?? throw new ArgumentNullException("Supabase:Url is missing");
        _supabaseKey = configuration["Supabase:Key"] ?? throw new ArgumentNullException("Supabase:Key is missing");
        _bucketName = configuration["Supabase:StorageBucket"] ?? "storage-iff-road";

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };

        _supabase = new Client(_supabaseUrl, _supabaseKey, options);
    }

    public async Task InitializeAsync()
    {
        await _supabase.InitializeAsync();
    }

    public async Task<Result<string>> CreateImage(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            return Result.Fail("Arquivo vazio ou inválido");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
            
        if (!allowedExtensions.Contains(fileExtension))
            return Result.Fail($"Tipo de arquivo não suportado: {fileExtension}");
        
        if (file.Length > 5 * 1024 * 1024) // 5MB
            return Result.Fail("Arquivo muito grande. Máximo 5MB");
        
        var sanitizedFileName = SanitizeFileName(file.FileName);
        var fileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
        var filePath = $"{folderName}/{fileName}";

        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }
        
        var response = await _supabase.Storage
            .From(_bucketName)
            .Upload(fileBytes, filePath, new FileOptions() 
            { 
                Upsert = false,
                ContentType = file.ContentType
            });
        
        if (string.IsNullOrEmpty(response))
            return Result.Fail("Falha no upload - resposta vazia do servidor");
        
        var publicUrl = _supabase.Storage
            .From(_bucketName)
            .GetPublicUrl(filePath);
        
        return Result.Ok(publicUrl);
    }

    public async Task<Result<bool>> DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return Result.Fail("URL da imagem vazia");

        var filePathResult = ExtractFilePathFromUrl(imageUrl);
        if (filePathResult.IsFailed)
            return filePathResult.ToResult<bool>();

        var filePath = filePathResult.Value;

        try 
        {
            await _supabase.Storage
                .From(_bucketName)
                .Remove(new List<string> { filePath });

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Erro ao deletar imagem do Supabase: {ex.Message}");
        }
    }
    
    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "file";

        var normalizedFileName = RemoveDiacritics(fileName);
        normalizedFileName = normalizedFileName.Replace(" ", "_");
        normalizedFileName = Regex.Replace(normalizedFileName, @"[^a-zA-Z0-9._-]", "");
        normalizedFileName = Regex.Replace(normalizedFileName, @"_{2,}", "_");
        normalizedFileName = Regex.Replace(normalizedFileName, @"-{2,}", "-");
        normalizedFileName = Regex.Replace(normalizedFileName, @"\.{2,}", ".");

        var nameParts = normalizedFileName.Split('.');
        var baseName = nameParts[0];
        var extension = nameParts.Length > 1 ? "." + nameParts[nameParts.Length - 1] : "";

        if (baseName.Length > 100)
            baseName = baseName.Substring(0, 100);

        normalizedFileName = baseName + extension;

        if (string.IsNullOrEmpty(normalizedFileName) || normalizedFileName == ".")
            return "file";

        return normalizedFileName;
    }
    
    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        try
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
        catch (Exception)
        {
            return text;
        }
    }
    
    private Result<string> ExtractFilePathFromUrl(string imageUrl)
    {
        try 
        {
            var uri = new Uri(imageUrl);
            var path = uri.LocalPath;
            
            var bucketSegment = $"/{_bucketName}/";
            var bucketIndex = path.IndexOf(bucketSegment);
            
            if (bucketIndex == -1)
                return Result.Fail($"Formato de URL inválido. O bucket '{_bucketName}' não foi encontrado na URL.");
            
            return Result.Ok(path.Substring(bucketIndex + bucketSegment.Length));
        }
        catch (Exception)
        {
            return Result.Fail("URL da imagem em formato inválido.");
        }
    }
}