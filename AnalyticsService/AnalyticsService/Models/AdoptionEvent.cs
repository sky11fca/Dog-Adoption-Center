namespace AnalyticsService.Models;

public class AdoptionEvent
{
    public Guid PetId { get; set; }
    public Guid UserId { get; set; }
    public Guid ShelterId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
