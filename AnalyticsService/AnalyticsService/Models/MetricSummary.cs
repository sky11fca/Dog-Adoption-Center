namespace AnalyticsService.Models;

public class MetricSummary
{
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
}

public class AdoptionTrendPoint
{
    public DateTimeOffset Date { get; set; }
    public int Adoptions { get; set; }
    public int Applications { get; set; }
    public int Views { get; set; }
}
