using Azure.Messaging.ServiceBus;
using NotificationService.Options;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"]
    ?? throw new InvalidOperationException("Missing ServiceBus__ConnectionString.");

builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<ApplicationStatusWorker>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    Service = "NotificationService",
    Responsibilities = new[] { "email-notifications", "service-bus-consumer" }
}));

app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));

app.Run();
