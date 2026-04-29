using AnalyticsService.Models;

namespace AnalyticsService.Services;

public interface IAnalyticsService
{
    Task TrackEventAsync(AdoptionEvent adoptionEvent, CancellationToken cancellationToken = default);
    Task<IEnumerable<AdoptionTrendPoint>> GetTrendsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
    Task<IEnumerable<MetricSummary>> GetSiteMetricsAsync(CancellationToken cancellationToken = default);
}
