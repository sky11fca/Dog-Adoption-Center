namespace AdoptionManager.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishApplicationStatusChangedAsync(
        Guid applicationId,
        Guid userId,
        string userEmail,
        string userName,
        string petName,
        string oldStatus,
        string newStatus,
        CancellationToken cancellationToken = default);
}
