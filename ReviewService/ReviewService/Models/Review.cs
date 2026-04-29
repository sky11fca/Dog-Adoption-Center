namespace ReviewService.Models;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShelterId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
