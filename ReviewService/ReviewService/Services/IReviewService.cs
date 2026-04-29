using ReviewService.Contracts;

namespace ReviewService.Services;

public interface IReviewService
{
    Task<IEnumerable<ReviewResponse>> GetByShelterAsync(Guid shelterId, CancellationToken cancellationToken);
    Task<ReviewResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ReviewResponse> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken);
    Task<ReviewResponse?> UpdateAsync(Guid id, UpdateReviewRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<ShelterRatingSummary> GetShelterSummaryAsync(Guid shelterId, CancellationToken cancellationToken);
}
