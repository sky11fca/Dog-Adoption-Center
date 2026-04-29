using PetManagementApi.Models;

namespace PetManagementApi.Contracts;

public static class PetMappings
{
    public static PetResponse ToResponse(this Pet pet)
    {
        var photos = pet.Photos
            .OrderByDescending(photo => photo.IsPrimary)
            .ThenByDescending(photo => photo.UploadedAt)
            .Select(photo => photo.ToResponse())
            .ToList();

        return new PetResponse(
            pet.Id,
            pet.Name,
            pet.Breed,
            pet.AgeYears,
            pet.Temperament,
            pet.Location,
            pet.Description,
            pet.Status,
            pet.CreatedAt,
            pet.UpdatedAt,
            photos);
    }

    public static PetPhotoResponse ToResponse(this PetPhoto photo)
    {
        return new PetPhotoResponse(
            photo.Id,
            photo.BlobName,
            photo.Url,
            photo.ContentType,
            photo.SizeBytes,
            photo.IsPrimary,
            photo.UploadedAt);
    }
}
