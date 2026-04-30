using System.Text.Json;
using AdoptionManager.Application.Interfaces;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace AdoptionManager.Infrastructure.Messaging;

public class ServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusEventPublisher> _logger;

    public ServiceBusEventPublisher(ServiceBusClient client, string queueName, ILogger<ServiceBusEventPublisher> logger)
    {
        _sender = client.CreateSender(queueName);
        _logger = logger;
    }

    public async Task PublishApplicationStatusChangedAsync(
        Guid applicationId,
        Guid userId,
        string userEmail,
        string userName,
        string petName,
        string oldStatus,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            ApplicationId = applicationId,
            UserId = userId,
            UserEmail = userEmail,
            UserName = userName,
            PetName = petName,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedAt = DateTimeOffset.UtcNow
        });

        var message = new ServiceBusMessage(payload) { ContentType = "application/json" };

        await _sender.SendMessageAsync(message, cancellationToken);
        _logger.LogInformation("Published status change {Old}->{New} for application {Id}", oldStatus, newStatus, applicationId);
    }

    public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
}
