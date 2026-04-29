namespace PetManagementApi.Models;

public class PetPhoto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    public string BlobName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadedAt { get; set; }
}
