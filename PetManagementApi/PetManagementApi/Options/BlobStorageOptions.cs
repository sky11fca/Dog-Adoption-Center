namespace PetManagementApi.Options;

public class BlobStorageOptions
{
    public string AccountName { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "pet-photos";
    public string? PublicBaseUrl { get; set; }
    public int UploadSasMinutes { get; set; } = 15;
    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;
}
