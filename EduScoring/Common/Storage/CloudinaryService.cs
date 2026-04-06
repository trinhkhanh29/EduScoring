using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EduScoring.Common.Storage;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var cloudName = config["CloudinarySettings:CloudName"];
        var apiKey = config["CloudinarySettings:ApiKey"];
        var apiSecret = config["CloudinarySettings:ApiSecret"];
        Console.WriteLine("CloudName: " + config["CloudinarySettings:CloudName"]);
        Console.WriteLine("ApiKey: " + config["CloudinarySettings:ApiKey"]);
        Console.WriteLine("ApiSecret: " + config["CloudinarySettings:ApiSecret"]);

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new InvalidOperationException("Cloudinary configuration is missing or incomplete.");
        }

        var acc = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(acc);

    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is null or empty.", nameof(file));
        }

        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "EduScoring/Submissions"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult?.SecureUrl == null)
        {
            throw new InvalidOperationException($"Upload failed: {uploadResult?.Error?.Message ?? "Unknown error"}");

        }

        return uploadResult.SecureUrl.ToString();
    }
}