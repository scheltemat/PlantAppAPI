using Microsoft.EntityFrameworkCore;
using PlantAppServer.Models;

public interface IWateringService
{
    Task<WaterPlantResult> WaterPlantAsync(int userId, int plantId);
    DateOnly CalculateNextWateringDate(UserPlant userPlant, DateOnly wateringDate);
}

public class WateringService : IWateringService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WateringService> _logger;

    public WateringService(ApplicationDbContext context, ILogger<WateringService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WaterPlantResult> WaterPlantAsync(int userId, int plantId)
    {
        try
        {
            // Get the user's plant with plant details
            var userPlant = await _context.UserPlants
                .Include(up => up.Plant)
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PlantId == plantId);

            if (userPlant == null)
                return new WaterPlantResult { Success = false, ErrorMessage = "Plant not found in your garden" };

            // Water the plant
            var today = DateOnly.FromDateTime(DateTime.Today);
            userPlant.LastWatered = today;
            userPlant.NextWatering = CalculateNextWateringDate(userPlant, today);

            await _context.SaveChangesAsync();

            return new WaterPlantResult
            {
                Success = true,
                LastWatered = userPlant.LastWatered,
                NextWatering = userPlant.NextWatering,
                WaterRequirement = userPlant.Plant.WaterRequirement
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error watering plant {PlantId} for user {UserId}", plantId, userId);
            return new WaterPlantResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred while watering your plant" 
            };
        }
    }

    public DateOnly CalculateNextWateringDate(UserPlant userPlant, DateOnly wateringDate)
    {
        // Get the plant's water requirement
        var waterRequirement = userPlant.Plant.WaterRequirement?.Trim() ?? "Moist";
        
        // Calculate next watering date based on the actual API values
        return waterRequirement switch
        {
            "Dry, Moist" => wateringDate.AddDays(10),  // Plants that prefer dry soil watered less frequently
            "Moist" => wateringDate.AddDays(5),        // Moist-loving plants watered more frequently
            _ => wateringDate.AddDays(7)               // Default fallback
        };
    }
}

public class WaterPlantResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateOnly? LastWatered { get; set; }
    public DateOnly? NextWatering { get; set; }
    public string? WaterRequirement { get; set; }
}