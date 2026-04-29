using Microsoft.EntityFrameworkCore;
using ReviewService.Contracts;
using ReviewService.Data;
using ReviewService.Models;

namespace ReviewService.Services;

public class ReviewServiceImpl : IReviewService
{
    private readonly ReviewDbContext _db;

    public ReviewServiceImpl(ReviewDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ReviewResponse>> GetByShelterAsync(Guid shelterId, CancellationToken cancellationToken)
    {
        return await _db.Reviews
            .Where(r => r.ShelterId == shelterId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => ToResponse(r))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReviewResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var review = await _db.Reviews.FindAsync([id], cancellationToken);
        return review is null ? null : ToResponse(review);
    }

    public async Task<ReviewResponse> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken)
    {
        if (request.Rating is < 1 or > 5)
            throw new ArgumentException("Rating must be between 1 and 5.");

        var review = new Review
        {
            ShelterId = request.ShelterId,
            UserId = request.UserId,
            UserName = request.UserName,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(review);
    }

    public async Task<ReviewResponse?> UpdateAsync(Guid id, UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        if (request.Rating is < 1 or > 5)
            throw new ArgumentException("Rating must be between 1 and 5.");

        var review = await _db.Reviews.FindAsync([id], cancellationToken);
        if (review is null) return null;

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(review);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var review = await _db.Reviews.FindAsync([id], cancellationToken);
        if (review is null) return false;

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ShelterRatingSummary> GetShelterSummaryAsync(Guid shelterId, CancellationToken cancellationToken)
    {
        var reviews = await _db.Reviews
            .Where(r => r.ShelterId == shelterId)
            .ToListAsync(cancellationToken);

        var avg = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0.0;
        return new ShelterRatingSummary(shelterId, Math.Round(avg, 2), reviews.Count);
    }

    private static ReviewResponse ToResponse(Review r) =>
        new(r.Id, r.ShelterId, r.UserId, r.UserName, r.Rating, r.Comment, r.CreatedAt, r.UpdatedAt);
}
