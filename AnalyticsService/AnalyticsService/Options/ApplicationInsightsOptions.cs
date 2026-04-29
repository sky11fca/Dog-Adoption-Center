namespace AnalyticsService.Options;

public class ApplicationInsightsOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string WorkspaceId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
