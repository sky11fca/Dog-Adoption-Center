namespace NotificationService.Models;

public class ApplicationStatusChangedEvent
{
    public Guid ApplicationId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
}
