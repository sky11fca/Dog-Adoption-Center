using AnalyticsService.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace AnalyticsService.Services;

public class AnalyticsServiceImpl : IAnalyticsService
{
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<AnalyticsServiceImpl> _logger;

    private readonly List<AdoptionEvent> _events = new();

    public AnalyticsServiceImpl(TelemetryClient telemetry, ILogger<AnalyticsServiceImpl> logger)
    {
        _telemetry = telemetry;
        _logger = logger;
        SeedDemoData();
    }

    private void SeedDemoData()
    {
        var rng = new Random(42);
        var petIds = Enumerable.Range(1, 6).Select(i => new Guid($"00000000-0000-0000-0000-00000000000{i}")).ToArray();
        var shelterId = new Guid("00000000-0000-0000-0000-000000000001");
        var userId = new Guid("00000000-0000-0000-0000-000000000002");

        for (int daysAgo = 29; daysAgo >= 8; daysAgo--)
        {
            var day = DateTimeOffset.UtcNow.AddDays(-daysAgo);

            for (int v = 0; v < rng.Next(8, 25); v++)
                _events.Add(new AdoptionEvent { PetId = petIds[rng.Next(petIds.Length)], UserId = userId, ShelterId = shelterId, EventType = "pet.viewed", OccurredAt = day, Metadata = new() });

            for (int a = 0; a < rng.Next(1, 6); a++)
                _events.Add(new AdoptionEvent { PetId = petIds[rng.Next(petIds.Length)], UserId = userId, ShelterId = shelterId, EventType = "application.submitted", OccurredAt = day, Metadata = new() });

            if (rng.Next(0, 3) == 0)
                _events.Add(new AdoptionEvent { PetId = petIds[rng.Next(petIds.Length)], UserId = userId, ShelterId = shelterId, EventType = "adoption.completed", OccurredAt = day, Metadata = new() });
        }
    }

    public Task TrackEventAsync(AdoptionEvent adoptionEvent, CancellationToken cancellationToken = default)
    {
        var telemetryEvent = new EventTelemetry(adoptionEvent.EventType);
        telemetryEvent.Properties["PetId"] = adoptionEvent.PetId.ToString();
        telemetryEvent.Properties["UserId"] = adoptionEvent.UserId.ToString();
        telemetryEvent.Properties["ShelterId"] = adoptionEvent.ShelterId.ToString();
        telemetryEvent.Timestamp = adoptionEvent.OccurredAt;

        foreach (var (key, value) in adoptionEvent.Metadata)
            telemetryEvent.Properties[key] = value;

        _telemetry.TrackEvent(telemetryEvent);

        lock (_events) _events.Add(adoptionEvent);

        _logger.LogInformation("Tracked event {EventType} for pet {PetId}", adoptionEvent.EventType, adoptionEvent.PetId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AdoptionTrendPoint>> GetTrendsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        List<AdoptionEvent> snapshot;
        lock (_events) snapshot = _events.Where(e => e.OccurredAt >= from && e.OccurredAt <= to).ToList();

        var points = snapshot
            .GroupBy(e => e.OccurredAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new AdoptionTrendPoint
            {
                Date = new DateTimeOffset(g.Key, TimeSpan.Zero),
                Adoptions = g.Count(e => e.EventType == "adoption.completed"),
                Applications = g.Count(e => e.EventType == "application.submitted"),
                Views = g.Count(e => e.EventType == "pet.viewed")
            });

        return Task.FromResult<IEnumerable<AdoptionTrendPoint>>(points);
    }

    public Task<IEnumerable<MetricSummary>> GetSiteMetricsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddDays(-7);

        List<AdoptionEvent> snapshot;
        lock (_events) snapshot = _events.Where(e => e.OccurredAt >= windowStart).ToList();

        var metrics = new List<MetricSummary>
        {
            new() { MetricName = "adoptions_last_7d", Value = snapshot.Count(e => e.EventType == "adoption.completed"), From = windowStart, To = now },
            new() { MetricName = "applications_last_7d", Value = snapshot.Count(e => e.EventType == "application.submitted"), From = windowStart, To = now },
            new() { MetricName = "pet_views_last_7d", Value = snapshot.Count(e => e.EventType == "pet.viewed"), From = windowStart, To = now },
        };

        return Task.FromResult<IEnumerable<MetricSummary>>(metrics);
    }
}
