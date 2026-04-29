using AnalyticsService.Models;
using AnalyticsService.Services;

var builder = WebApplication.CreateBuilder(args);

var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
        options.ConnectionString = appInsightsConnectionString);
}
else
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddSingleton<IAnalyticsService, AnalyticsServiceImpl>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();
app.UseCors("Frontend");

app.MapGet("/", () => Results.Ok(new
{
    Service = "AnalyticsService",
    Responsibilities = new[] { "adoption-trends", "site-metrics", "application-insights" }
}));

app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));

var analytics = app.MapGroup("/analytics");

analytics.MapPost("/events", async (AdoptionEvent evt, IAnalyticsService svc, CancellationToken ct) =>
{
    await svc.TrackEventAsync(evt, ct);
    return Results.Accepted();
});

analytics.MapGet("/trends", async (
    DateTimeOffset? from,
    DateTimeOffset? to,
    IAnalyticsService svc,
    CancellationToken ct) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-30);
    var end = to ?? DateTimeOffset.UtcNow;
    return Results.Ok(await svc.GetTrendsAsync(start, end, ct));
});

analytics.MapGet("/metrics", async (IAnalyticsService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetSiteMetricsAsync(ct)));

app.Run();
