using Microsoft.EntityFrameworkCore;
using PetManagementApi.Models;

namespace PetManagementApi.Data;

public class PetManagementContext(DbContextOptions<PetManagementContext> options) : DbContext(options)
{
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<PetPhoto> PetPhotos => Set<PetPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.ToTable("Pets");
            entity.HasKey(pet => pet.Id);

            entity.Property(pet => pet.Name).HasMaxLength(120).IsRequired();
            entity.Property(pet => pet.Breed).HasMaxLength(120).IsRequired();
            entity.Property(pet => pet.Temperament).HasMaxLength(160).IsRequired();
            entity.Property(pet => pet.Location).HasMaxLength(160).IsRequired();
            entity.Property(pet => pet.Description).HasMaxLength(2000).IsRequired();
            entity.Property(pet => pet.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

            entity.HasIndex(pet => pet.Breed);
            entity.HasIndex(pet => pet.Location);
            entity.HasIndex(pet => pet.Status);
            entity.HasIndex(pet => pet.CreatedAt);
        });

        modelBuilder.Entity<PetPhoto>(entity =>
        {
            entity.ToTable("PetPhotos");
            entity.HasKey(photo => photo.Id);

            entity.Property(photo => photo.BlobName).HasMaxLength(512).IsRequired();
            entity.Property(photo => photo.Url).HasMaxLength(2048).IsRequired();
            entity.Property(photo => photo.ContentType).HasMaxLength(120).IsRequired();

            entity.HasIndex(photo => photo.BlobName).IsUnique();
            entity.HasOne(photo => photo.Pet)
                .WithMany(pet => pet.Photos)
                .HasForeignKey(photo => photo.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
