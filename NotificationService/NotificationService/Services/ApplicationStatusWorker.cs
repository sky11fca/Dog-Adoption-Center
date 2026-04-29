using System.Text.Json;
using Azure.Messaging.ServiceBus;
using NotificationService.Models;

namespace NotificationService.Services;

public class ApplicationStatusWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationStatusWorker> _logger;

    public ApplicationStatusWorker(
        ServiceBusClient client,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ApplicationStatusWorker> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _configuration["ServiceBus:ApplicationStatusQueueName"] ?? "application-status-changed";

        await using var processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 4,
            AutoCompleteMessages = false
        });

        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync += HandleErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Listening on Service Bus queue: {Queue}", queueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);

        await processor.StopProcessingAsync(CancellationToken.None);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var payload = args.Message.Body.ToString();
            var evt = JsonSerializer.Deserialize<ApplicationStatusChangedEvent>(payload);

            if (evt is null)
            {
                _logger.LogWarning("Received null event, skipping");
                await args.DeadLetterMessageAsync(args.Message, "NullPayload", "Deserialized to null");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var subject = $"Your adoption application status changed to {evt.NewStatus}";
            var body = BuildEmailBody(evt);

            await emailService.SendAsync(evt.UserEmail, evt.UserName, subject, body, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process application status message");
            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processor error on {Source}", args.ErrorSource);
        return Task.CompletedTask;
    }

    private static string BuildEmailBody(ApplicationStatusChangedEvent evt) => $"""
        <h2>Adoption Application Update</h2>
        <p>Hi {evt.UserName},</p>
        <p>Your adoption application for <strong>{evt.PetName}</strong> has been updated.</p>
        <ul>
            <li><strong>Previous status:</strong> {evt.OldStatus}</li>
            <li><strong>New status:</strong> {evt.NewStatus}</li>
            <li><strong>Changed at:</strong> {evt.ChangedAt:f}</li>
        </ul>
        <p>Thank you for using the Dog Adoption Center!</p>
        """;
}
