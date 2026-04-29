using AdoptionManager.Application.Commands;
using AdoptionManager.Domain.Interfaces;
using AdoptionManager.Infrastructure.Persistence;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(SubmitApplicationCommand).Assembly);
});

// Configure CosmosDB
var cosmosUri = Environment.GetEnvironmentVariable("COSMOS_URI") ?? builder.Configuration["CosmosDb:Uri"];
var cosmosKey = Environment.GetEnvironmentVariable("COSMOS_KEY");

if (string.IsNullOrEmpty(cosmosUri))
{
    throw new InvalidOperationException("Cosmos URI is missing.");
}

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    if (!string.IsNullOrEmpty(cosmosKey))
    {
        // Use explicit key if provided (e.g., in Docker/.env)
        return new CosmosClient(cosmosUri, cosmosKey);
    }
    
    // Fallback to DefaultAzureCredential (e.g., Managed Identity in Azure)
    return new CosmosClient(cosmosUri, new DefaultAzureCredential());
});

builder.Services.AddScoped<IAdoptionRepository>(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    return new CosmosAdoptionRepository(client, "AdoptionDb", "Applications");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure Database and Container are created
using (var scope = app.Services.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<CosmosClient>();
    var database = await client.CreateDatabaseIfNotExistsAsync("AdoptionDb");
    await database.Database.CreateContainerIfNotExistsAsync("Applications", "/id");
}

app.Run();
