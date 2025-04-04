using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Configure CORS to allow requests from different origins based on the environment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        var allowedOrigin = builder.Environment.IsDevelopment()
            ? "http://localhost:5000"  // Development URL
            : "https://www.yourapp.com";  // Production URL
        
        policy.WithOrigins(allowedOrigin)
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

// Add controllers to the container
builder.Services.AddControllers();

var app = builder.Build();

// Enable middleware to serve generated Swagger as a JSON endpoint
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // This will allow OpenAPI docs in the development environment
}

// Use CORS to allow communication between the Blazor app and the API
app.UseCors("AllowBlazorApp");

// Use routing and map controllers
app.UseRouting();

app.MapControllers();  // This maps all controller-based routes

// Simple health check endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new { message = "I am healthy" });
}).WithName("HealthCheck");

app.Run();
