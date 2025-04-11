using System.Text;
using System.Text.Json;
using System.Net.Http.Headers; 
using PlantAppServer.Models;

namespace PlantAppServer.Services;

public interface IPermapeopleService
{
    Task<JsonDocument> SearchPlantsAsync(string query);
    Task<JsonDocument?> GetPlantByIdAsync(int id);
}

public class PermapeopleService : IPermapeopleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PermapeopleService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PermapeopleService(HttpClient httpClient, ILogger<PermapeopleService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var keyId = Environment.GetEnvironmentVariable("PERMAPEOPLE_KEY_ID");
        var keySecret = Environment.GetEnvironmentVariable("PERMAPEOPLE_KEY_SECRET");

        if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
        {
            _logger.LogError("API credentials not found in environment variables");
            throw new InvalidOperationException("API credentials not configured");
        }

        _httpClient.BaseAddress = new Uri("https://permapeople.org/api/");
        _httpClient.DefaultRequestHeaders.Add("x-permapeople-key-id", keyId);
        _httpClient.DefaultRequestHeaders.Add("x-permapeople-key-secret", keySecret);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<JsonDocument> SearchPlantsAsync(string query)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { q = query }, _jsonOptions),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("search", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Search API error: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching plants for query: {query}", query);
            throw;
        }
    }

    public async Task<JsonDocument?> GetPlantByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"plants/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Plant ID {Id} not found", id);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Plant API error for ID {Id}: {StatusCode} - {Content}", 
                    id, response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plant by ID: {Id}", id);
            throw;
        }
    }
}