using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure CORS to allow requests from different origins based on the environment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        var allowedOrigin = builder.Environment.IsDevelopment()
            ? "http://localhost:5000"  // Development URL
            : "https://www.yourapp.com";  // Production URL
        
        policy.WithOrigins(allowedOrigin)  // Dynamically set allowed origin based on environment
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DbContext to use the appropriate connection string based on environment
var connectionString = builder.Environment.IsDevelopment() ?
    builder.Configuration.GetConnectionString("DefaultConnection") :
    builder.Configuration.GetConnectionString("ProductionConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Use CORS to allow communication between the Blazor app and the API
app.UseCors("AllowBlazorApp");

// Create a route group for API and prefix it with /api
var api = app.MapGroup("/api");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    api.MapOpenApi(); // This will allow OpenAPI docs in the development environment
}

// Define your API endpoints under the /api route group
api.MapGet("/", () =>
{
    return Results.Ok(new { message = "I am healthy" }); // A simple health check response
}).WithName("HealthCheck");

app.Run();
