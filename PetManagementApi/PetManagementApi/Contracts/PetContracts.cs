using PetManagementApi.Models;

namespace PetManagementApi.Contracts;

public sealed record CreatePetRequest(
    string Name,
    string Breed,
    int AgeYears,
    string Temperament,
    string Location,
    string Description,
    PetStatus Status = PetStatus.Available);

public sealed record UpdatePetRequest(
    string Name,
    string Breed,
    int AgeYears,
    string Temperament,
    string Location,
    string Description,
    PetStatus Status);

public sealed record PetSearchQuery(
    string? Breed,
    string? Location,
    string? Temperament,
    PetStatus? Status,
    int? MinAge,
    int? MaxAge,
    string? Q,
    int Page = 1,
    int PageSize = 20);

public sealed record PetResponse(
    Guid Id,
    string Name,
    string Breed,
    int AgeYears,
    string Temperament,
    string Location,
    string Description,
    PetStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PetPhotoResponse> Photos);

public sealed record PetPhotoResponse(
    Guid Id,
    string BlobName,
    string Url,
    string ContentType,
    long SizeBytes,
    bool IsPrimary,
    DateTime UploadedAt);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CreatePhotoUploadRequest(
    string FileName,
    string ContentType,
    long SizeBytes,
    bool IsPrimary = false);

public sealed record PhotoUploadTicketResponse(
    Guid PetId,
    string BlobName,
    string BlobUrl,
    string UploadUrl,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> RequiredHeaders);

public sealed record CompletePhotoUploadRequest(
    string BlobName,
    string ContentType,
    long SizeBytes,
    bool IsPrimary = false);
