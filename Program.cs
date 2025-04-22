using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;  // Required to load .env file
using System.Text;
using PlantAppServer.Models;
using PlantAppServer.Services;
using PlantAppServer.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the .env file
Env.Load();

// Add services to the container
builder.Services.AddOpenApi();

// Add HttpClient support (THIS IS THE FIX)
builder.Services.AddHttpClient();  // Add this line

// Configure CORS to allow requests from different origins based on the environment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var allowedOrigin = builder.Environment.IsDevelopment()
            ? "http://localhost:4200"  // Development URL
            : "https://www.yourapp.com";  // Production URL
        
        policy.WithOrigins(allowedOrigin)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();  // Important: Allow sending cookies
    });
});

// Configure DbContext to use the appropriate connection string based on environment
var connectionString = builder.Environment.IsDevelopment() ? 
    builder.Configuration.GetConnectionString("DefaultConnection") : 
    builder.Configuration.GetConnectionString("ProductionConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Retrieve JWT settings from environment variables
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var expirationMinutesStr = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES");

// Ensure that all necessary JWT settings are present
if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(expirationMinutesStr))
{
    throw new InvalidOperationException("JWT settings (secret key, issuer, audience, expiration) must be set in environment variables.");
}

var jwtSettings = new JwtSettings
{
    SecretKey = secretKey,
    Issuer = issuer,
    Audience = audience,
    ExpirationInMinutes = int.Parse(expirationMinutesStr)  // Default to 60 if parsing fails
};

// Register JwtSettings for Dependency Injection
builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSettings.SecretKey;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationInMinutes = jwtSettings.ExpirationInMinutes;
});

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add controllers to the container
builder.Services.AddControllers();

builder.Services.AddScoped<IPermapeopleService, PermapeopleService>();
builder.Services.AddScoped<IWateringService, WateringService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddHostedService<ReminderScheduler>();



// Register Swagger services
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PlantApp API",
        Version = "v1",
        Description = "API for managing plant data"
    });
});

var app = builder.Build();

// Enable Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();  // Add Swagger UI for API documentation
    app.UseDeveloperExceptionPage(); 
}

// Use CORS to allow communication between the Blazor app and the API
app.UseCors("AllowAngularApp");

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Simple health check endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new { message = "I am healthy" });
}).WithName("HealthCheck");

app.Run();