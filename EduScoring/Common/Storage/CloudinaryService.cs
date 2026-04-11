using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace EduScoring.Common.Storage;

// ── Strongly-typed config (bind từ appsettings.json)
public sealed class CloudinarySettings
{
    public const string SectionName = "CloudinarySettings";

    public string CloudName { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
    public string UploadFolder { get; init; } = "EduScoring/Submissions";
}

// ── Interface
public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken ct = default);
    Task DeleteImageAsync(string publicId, CancellationToken ct = default);
}

public sealed class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryService> _logger;

    // Các định dạng ảnh được phép upload
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        // ── Validate config tại startup
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(_settings.CloudName)) errors.Add("CloudName");
        if (string.IsNullOrWhiteSpace(_settings.ApiKey)) errors.Add("ApiKey");
        if (string.IsNullOrWhiteSpace(_settings.ApiSecret)) errors.Add("ApiSecret");

        if (errors.Count > 0)
        {
            var msg = $"[Cloudinary] Thiếu config: {string.Join(", ", errors)}. Kiểm tra appsettings.json.";
            _logger.LogCritical(msg);
            throw new InvalidOperationException(msg);
        }

        _cloudinary = new Cloudinary(new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret));
        _cloudinary.Api.Secure = true;

        _logger.LogInformation("[Cloudinary] Khởi tạo thành công | CloudName={CloudName} | Folder={Folder}",
            _settings.CloudName, _settings.UploadFolder);
    }

    // ── Upload
    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken ct = default)
    {
        ValidateFile(file);

        _logger.LogInformation("[Cloudinary] Bắt đầu upload | File={Name} | Size={Size} bytes",
            file.FileName, file.Length);

        try
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = _settings.UploadFolder,
                UseFilenameAsDisplayName = false,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.Error is not null)
            {
                _logger.LogError("[Cloudinary] Upload thất bại | File={Name} | Lỗi={Error}",
                    file.FileName, result.Error.Message);
                throw new InvalidOperationException($"Cloudinary upload thất bại: {result.Error.Message}");
            }

            _logger.LogInformation("[Cloudinary] Upload thành công | PublicId={PublicId} | Url={Url}",
                result.PublicId, result.SecureUrl);

            return result.SecureUrl.ToString();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cloudinary] Lỗi không xác định khi upload file '{Name}'.", file.FileName);
            throw new InvalidOperationException($"Lỗi khi upload ảnh '{file.FileName}'.", ex);
        }
    }

    // ── Delete
    public async Task DeleteImageAsync(string publicId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            _logger.LogWarning("[Cloudinary] DeleteImageAsync được gọi với publicId rỗng.");
            throw new ArgumentException("publicId không được để trống.", nameof(publicId));
        }

        _logger.LogInformation("[Cloudinary] Xóa ảnh | PublicId={PublicId}", publicId);

        try
        {
            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

            if (result.Result != "ok")
            {
                _logger.LogWarning("[Cloudinary] Xóa không thành công | PublicId={PublicId} | Kết quả={Result}",
                    publicId, result.Result);
            }
            else
            {
                _logger.LogInformation("[Cloudinary] Đã xóa thành công | PublicId={PublicId}", publicId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cloudinary] Lỗi khi xóa ảnh '{PublicId}'.", publicId);
            throw new InvalidOperationException($"Lỗi khi xóa ảnh '{publicId}'.", ex);
        }
    }

    // ── Private helpers
    private void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("[Cloudinary] File null hoặc rỗng.");
            throw new ArgumentException("File không được null hoặc rỗng.", nameof(file));
        }

        if (file.Length > MaxFileSizeBytes)
        {
            _logger.LogWarning("[Cloudinary] File quá lớn | Size={Size} | Max={Max}", file.Length, MaxFileSizeBytes);
            throw new ArgumentException($"File vượt quá giới hạn {MaxFileSizeBytes / 1024 / 1024} MB.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
        {
            _logger.LogWarning("[Cloudinary] Định dạng không hợp lệ | Extension={Ext}", ext);
            throw new ArgumentException($"Định dạng '{ext}' không được hỗ trợ. Chấp nhận: {string.Join(", ", AllowedExtensions)}");
        }
    }
}