using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementApi.Contracts;
using PetManagementApi.Data;
using PetManagementApi.Options;
using PetManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PetManagementContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PetDatabase");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Missing ConnectionStrings__PetDatabase.");
    }

    options.UseNpgsql(connectionString);
});

builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddSingleton<IBlobUploadService, BlobUploadService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

app.UseCors("Frontend");

if (app.Configuration.GetValue<bool>("PetManagement:AutoCreateDatabase"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PetManagementContext>();
    context.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Ok(new
{
    Service = "PetManagementApi",
    Responsibilities = new[] { "pet-listing", "media-upload-tickets", "search-discovery" }
}));

app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));

var pets = app.MapGroup("/pets");

pets.MapGet("/", async ([AsParameters] PetSearchQuery query, IPetService petService, CancellationToken cancellationToken) =>
{
    var result = await petService.SearchAsync(query, cancellationToken);
    return Results.Ok(result);
});

pets.MapGet("/{id:guid}", async (Guid id, IPetService petService, CancellationToken cancellationToken) =>
{
    var pet = await petService.GetByIdAsync(id, cancellationToken);
    return pet is null ? Results.NotFound() : Results.Ok(pet);
});

pets.MapPost("/", async (CreatePetRequest request, IPetService petService, CancellationToken cancellationToken) =>
{
    try
    {
        var pet = await petService.CreateAsync(request, cancellationToken);
        return Results.Created($"/pets/{pet.Id}", pet);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
});

pets.MapPut("/{id:guid}", async (Guid id, UpdatePetRequest request, IPetService petService, CancellationToken cancellationToken) =>
{
    try
    {
        var pet = await petService.UpdateAsync(id, request, cancellationToken);
        return pet is null ? Results.NotFound() : Results.Ok(pet);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
});

pets.MapDelete("/{id:guid}", async (Guid id, IPetService petService, CancellationToken cancellationToken) =>
{
    var deleted = await petService.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

pets.MapPost("/{petId:guid}/photos/upload-ticket", async (
    Guid petId,
    CreatePhotoUploadRequest request,
    IPetService petService,
    IBlobUploadService blobUploadService,
    CancellationToken cancellationToken) =>
{
    var pet = await petService.GetByIdAsync(petId, cancellationToken);
    if (pet is null)
    {
        return Results.NotFound();
    }

    try
    {
        var ticket = blobUploadService.CreateUploadTicket(petId, request);
        return Results.Ok(ticket);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

pets.MapPost("/{petId:guid}/photos/complete", async (
    Guid petId,
    CompletePhotoUploadRequest request,
    IPetService petService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var photo = await petService.AddPhotoAsync(petId, request, cancellationToken);
        return photo is null ? Results.NotFound() : Results.Created($"/pets/{petId}", photo);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

pets.MapDelete("/{petId:guid}/photos/{photoId:guid}", async (
    Guid petId,
    Guid photoId,
    IPetService petService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var deleted = await petService.DeletePhotoAsync(petId, photoId, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidOperationException exception)
    {
        return Results.Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
