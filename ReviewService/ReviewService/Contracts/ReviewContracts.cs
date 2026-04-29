namespace ReviewService.Contracts;

public record CreateReviewRequest(
    Guid ShelterId,
    Guid UserId,
    string UserName,
    int Rating,
    string Comment);

public record UpdateReviewRequest(
    int Rating,
    string Comment);

public record ReviewResponse(
    Guid Id,
    Guid ShelterId,
    Guid UserId,
    string UserName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record ShelterRatingSummary(
    Guid ShelterId,
    double AverageRating,
    int TotalReviews);
