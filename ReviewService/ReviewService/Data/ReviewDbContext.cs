using Microsoft.EntityFrameworkCore;
using ReviewService.Models;

namespace ReviewService.Data;

public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Rating).IsRequired();
            e.HasIndex(r => r.ShelterId);
            e.HasIndex(r => r.UserId);
        });
    }
}
