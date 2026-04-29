using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using PetManagementApi.Contracts;
using PetManagementApi.Options;

namespace PetManagementApi.Services;

public interface IBlobUploadService
{
    PhotoUploadTicketResponse CreateUploadTicket(Guid petId, CreatePhotoUploadRequest request);
    string GetBlobUrl(string blobName);
    Task DeleteBlobIfExistsAsync(string blobName, CancellationToken cancellationToken);
}

public class BlobUploadService(IOptions<BlobStorageOptions> options) : IBlobUploadService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly BlobStorageOptions _options = options.Value;

    public PhotoUploadTicketResponse CreateUploadTicket(Guid petId, CreatePhotoUploadRequest request)
    {
        ValidateOptions();
        ValidateImageRequest(request);

        var extension = GetSafeExtension(request.FileName, request.ContentType);
        var blobName = $"pets/{petId:N}/{Guid.NewGuid():N}{extension}";
        var blobClient = GetBlobClient(blobName);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(Math.Clamp(_options.UploadSasMinutes, 1, 60));

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = expiresAt,
            ContentType = request.ContentType
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);
        var uploadUrl = blobClient.GenerateSasUri(sasBuilder).ToString();

        return new PhotoUploadTicketResponse(
            petId,
            blobName,
            GetBlobUrl(blobName),
            uploadUrl,
            expiresAt,
            new Dictionary<string, string>
            {
                ["x-ms-blob-type"] = "BlockBlob",
                ["Content-Type"] = request.ContentType
            });
    }

    public string GetBlobUrl(string blobName)
    {
        ValidateOptions();

        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return $"{_options.PublicBaseUrl.TrimEnd('/')}/{blobName}";
        }

        return $"https://{_options.AccountName}.blob.core.windows.net/{_options.ContainerName}/{blobName}";
    }

    public async Task DeleteBlobIfExistsAsync(string blobName, CancellationToken cancellationToken)
    {
        ValidateOptions();
        await GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private BlobClient GetBlobClient(string blobName)
    {
        var credential = new StorageSharedKeyCredential(_options.AccountName, _options.AccountKey);
        var containerUri = new Uri($"https://{_options.AccountName}.blob.core.windows.net/{_options.ContainerName}");
        return new BlobContainerClient(containerUri, credential).GetBlobClient(blobName);
    }

    private void ValidateOptions()
    {
        if (IsMissing(_options.AccountName) ||
            IsMissing(_options.AccountKey) ||
            IsMissing(_options.ContainerName))
        {
            throw new InvalidOperationException("Blob storage is not configured. Set BlobStorage__AccountName, BlobStorage__AccountKey and BlobStorage__ContainerName.");
        }

        try
        {
            Convert.FromBase64String(_options.AccountKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("BlobStorage__AccountKey must be a valid Azure Storage account key.", exception);
        }
    }

    private void ValidateImageRequest(CreatePhotoUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException("FileName is required.");
        }

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            throw new ArgumentException("Only jpeg, png and webp images are accepted.");
        }

        if (request.SizeBytes <= 0 || request.SizeBytes > _options.MaxImageBytes)
        {
            throw new ArgumentException($"Image size must be between 1 byte and {_options.MaxImageBytes} bytes.");
        }
    }

    private static string GetSafeExtension(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is ".jpg" or ".jpeg" or ".png" or ".webp")
        {
            return extension == ".jpeg" ? ".jpg" : extension;
        }

        return contentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }

    private static bool IsMissing(string value)
    {
        return string.IsNullOrWhiteSpace(value) ||
            value.StartsWith("PLACEHOLDER", StringComparison.OrdinalIgnoreCase);
    }
}
