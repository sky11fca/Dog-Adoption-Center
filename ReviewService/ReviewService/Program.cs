using Microsoft.EntityFrameworkCore;
using Npgsql;
using ReviewService.Data;
using ReviewService.Services;
using ReviewService.Contracts;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ReviewDatabase")
    ?? throw new InvalidOperationException("Missing ConnectionStrings__ReviewDatabase.");

builder.Services.AddDbContext<ReviewDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IReviewService, ReviewServiceImpl>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();
app.UseCors("Frontend");

if (app.Configuration.GetValue<bool>("ReviewService:AutoCreateDatabase"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Ok(new
{
    Service = "ReviewService",
    Responsibilities = new[] { "shelter-reviews", "ratings" }
}));

app.MapGet("/health", () => Results.Ok(new { Status = "ok" }));

var reviews = app.MapGroup("/reviews");

reviews.MapGet("/shelter/{shelterId:guid}", async (Guid shelterId, IReviewService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetByShelterAsync(shelterId, ct)));

reviews.MapGet("/shelter/{shelterId:guid}/summary", async (Guid shelterId, IReviewService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetShelterSummaryAsync(shelterId, ct)));

reviews.MapGet("/{id:guid}", async (Guid id, IReviewService svc, CancellationToken ct) =>
{
    var review = await svc.GetByIdAsync(id, ct);
    return review is null ? Results.NotFound() : Results.Ok(review);
});

reviews.MapPost("/", async (CreateReviewRequest request, IReviewService svc, CancellationToken ct) =>
{
    try
    {
        var review = await svc.CreateAsync(request, ct);
        return Results.Created($"/reviews/{review.Id}", review);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

reviews.MapPut("/{id:guid}", async (Guid id, UpdateReviewRequest request, IReviewService svc, CancellationToken ct) =>
{
    try
    {
        var review = await svc.UpdateAsync(id, request, ct);
        return review is null ? Results.NotFound() : Results.Ok(review);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

reviews.MapDelete("/{id:guid}", async (Guid id, IReviewService svc, CancellationToken ct) =>
{
    var deleted = await svc.DeleteAsync(id, ct);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();
