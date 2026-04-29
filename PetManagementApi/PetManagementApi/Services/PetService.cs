using Microsoft.EntityFrameworkCore;
using PetManagementApi.Contracts;
using PetManagementApi.Data;
using PetManagementApi.Models;

namespace PetManagementApi.Services;

public interface IPetService
{
    Task<PagedResult<PetResponse>> SearchAsync(PetSearchQuery query, CancellationToken cancellationToken);
    Task<PetResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken);
    Task<PetResponse?> UpdateAsync(Guid id, UpdatePetRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<PetPhotoResponse?> AddPhotoAsync(Guid petId, CompletePhotoUploadRequest request, CancellationToken cancellationToken);
    Task<bool> DeletePhotoAsync(Guid petId, Guid photoId, CancellationToken cancellationToken);
}

public class PetService(PetManagementContext context, IBlobUploadService blobUploadService) : IPetService
{
    public async Task<PagedResult<PetResponse>> SearchAsync(PetSearchQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pets = context.Pets
            .Include(pet => pet.Photos)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Breed))
        {
            pets = pets.Where(pet => pet.Breed.Contains(query.Breed));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            pets = pets.Where(pet => pet.Location.Contains(query.Location));
        }

        if (!string.IsNullOrWhiteSpace(query.Temperament))
        {
            pets = pets.Where(pet => pet.Temperament.Contains(query.Temperament));
        }

        if (query.Status is not null)
        {
            pets = pets.Where(pet => pet.Status == query.Status);
        }

        if (query.MinAge is not null)
        {
            pets = pets.Where(pet => pet.AgeYears >= query.MinAge);
        }

        if (query.MaxAge is not null)
        {
            pets = pets.Where(pet => pet.AgeYears <= query.MaxAge);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            pets = pets.Where(pet =>
                pet.Name.Contains(query.Q) ||
                pet.Breed.Contains(query.Q) ||
                pet.Location.Contains(query.Q) ||
                pet.Temperament.Contains(query.Q) ||
                pet.Description.Contains(query.Q));
        }

        var totalCount = await pets.CountAsync(cancellationToken);
        var items = await pets
            .OrderByDescending(pet => pet.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PetResponse>(
            items.Select(pet => pet.ToResponse()).ToList(),
            page,
            pageSize,
            totalCount);
    }

    public async Task<PetResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pet = await context.Pets
            .Include(pet => pet.Photos)
            .AsNoTracking()
            .FirstOrDefaultAsync(pet => pet.Id == id, cancellationToken);

        return pet?.ToResponse();
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken)
    {
        ValidatePet(request.Name, request.Breed, request.AgeYears, request.Temperament, request.Location, request.Description);

        var now = DateTime.UtcNow;
        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Breed = request.Breed.Trim(),
            AgeYears = request.AgeYears,
            Temperament = request.Temperament.Trim(),
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Pets.Add(pet);
        await context.SaveChangesAsync(cancellationToken);

        return pet.ToResponse();
    }

    public async Task<PetResponse?> UpdateAsync(Guid id, UpdatePetRequest request, CancellationToken cancellationToken)
    {
        ValidatePet(request.Name, request.Breed, request.AgeYears, request.Temperament, request.Location, request.Description);

        var pet = await context.Pets
            .Include(existingPet => existingPet.Photos)
            .FirstOrDefaultAsync(existingPet => existingPet.Id == id, cancellationToken);

        if (pet is null)
        {
            return null;
        }

        pet.Name = request.Name.Trim();
        pet.Breed = request.Breed.Trim();
        pet.AgeYears = request.AgeYears;
        pet.Temperament = request.Temperament.Trim();
        pet.Location = request.Location.Trim();
        pet.Description = request.Description.Trim();
        pet.Status = request.Status;
        pet.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return pet.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var pet = await context.Pets
            .Include(existingPet => existingPet.Photos)
            .FirstOrDefaultAsync(existingPet => existingPet.Id == id, cancellationToken);

        if (pet is null)
        {
            return false;
        }

        foreach (var photo in pet.Photos)
        {
            await blobUploadService.DeleteBlobIfExistsAsync(photo.BlobName, cancellationToken);
        }

        context.Pets.Remove(pet);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PetPhotoResponse?> AddPhotoAsync(Guid petId, CompletePhotoUploadRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BlobName))
        {
            throw new ArgumentException("BlobName is required.");
        }

        if (!request.BlobName.StartsWith($"pets/{petId:N}/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("BlobName does not match this pet.");
        }

        if (!request.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("ContentType must be an image content type.");
        }

        if (request.SizeBytes <= 0)
        {
            throw new ArgumentException("SizeBytes must be positive.");
        }

        var pet = await context.Pets
            .Include(existingPet => existingPet.Photos)
            .FirstOrDefaultAsync(existingPet => existingPet.Id == petId, cancellationToken);

        if (pet is null)
        {
            return null;
        }

        if (request.IsPrimary)
        {
            foreach (var existingPhoto in pet.Photos)
            {
                existingPhoto.IsPrimary = false;
            }
        }

        var photo = new PetPhoto
        {
            Id = Guid.NewGuid(),
            PetId = petId,
            BlobName = request.BlobName.Trim(),
            Url = blobUploadService.GetBlobUrl(request.BlobName.Trim()),
            ContentType = request.ContentType.Trim(),
            SizeBytes = request.SizeBytes,
            IsPrimary = request.IsPrimary || pet.Photos.Count == 0,
            UploadedAt = DateTime.UtcNow
        };

        pet.Photos.Add(photo);
        pet.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return photo.ToResponse();
    }

    public async Task<bool> DeletePhotoAsync(Guid petId, Guid photoId, CancellationToken cancellationToken)
    {
        var photo = await context.PetPhotos
            .FirstOrDefaultAsync(existingPhoto => existingPhoto.PetId == petId && existingPhoto.Id == photoId, cancellationToken);

        if (photo is null)
        {
            return false;
        }

        await blobUploadService.DeleteBlobIfExistsAsync(photo.BlobName, cancellationToken);
        context.PetPhotos.Remove(photo);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidatePet(
        string name,
        string breed,
        int ageYears,
        string temperament,
        string location,
        string description)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(breed) ||
            string.IsNullOrWhiteSpace(temperament) ||
            string.IsNullOrWhiteSpace(location) ||
            string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Name, breed, temperament, location and description are required.");
        }

        if (ageYears < 0 || ageYears > 30)
        {
            throw new ArgumentException("AgeYears must be between 0 and 30.");
        }
    }
}
