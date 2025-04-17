using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantAppServer.Models;
using PlantAppServer.Models.DTOs;
using System.Security.Claims;

namespace PlantAppServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user-plants")]
    public class UserPlantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserPlantController> _logger;

        public UserPlantController(ApplicationDbContext context, ILogger<UserPlantController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // get all plants in the user's garden
        [HttpGet("")]
        public async Task<IActionResult> GetUserPlants()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

            try
            {
                var userPlants = await _context.UserPlants
                    .Include(up => up.Plant)
                    .Where(up => up.UserId == userId)
                    .Select(up => new PlantDto
                    {
                        Id = up.Plant.Id,
                        PermapeopleId = up.Plant.PermapeopleId,
                        Name = up.Plant.Name,
                        ImageUrl = up.Plant.ImageUrl,
                        WaterRequirement = up.Plant.WaterRequirement,
                        LightRequirement = up.Plant.LightRequirement,
                        LastWatered = up.LastWatered,
                        NextWatering = up.NextWatering,
                        NeedsWatering = !up.NextWatering.HasValue || 
                                        DateOnly.FromDateTime(DateTime.Today) >= up.NextWatering.Value
                    })
                    .ToListAsync();

                return Ok(userPlants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plants for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving your garden");
            }
        }

        // add a plant to the user's garden
        [HttpPost("")]
        public async Task<IActionResult> AddPlantToGarden([FromBody] PlantApiResponse plantApiResponse)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                ?? throw new UnauthorizedAccessException());

            try
            {
                // Check if the plant exists globally
                var plant = await _context.Plants
                    .FirstOrDefaultAsync(p => p.PermapeopleId == plantApiResponse.Id);

                if (plant == null)
                {
                    plant = new Plant
                    {
                        PermapeopleId = plantApiResponse.Id,
                        Name = plantApiResponse.Name,
                        ImageUrl = plantApiResponse.ImageUrl,
                        WaterRequirement = plantApiResponse.WaterRequirement, 
                        LightRequirement = plantApiResponse.LightRequirement 
                    };

                    _context.Plants.Add(plant);
                    await _context.SaveChangesAsync();
                }

                // Check if this user already has the plant
                var userPlantExists = await _context.UserPlants
                    .AnyAsync(up => up.UserId == userId && up.PlantId == plant.Id);

                if (userPlantExists)
                    return BadRequest("You already have this plant in your garden.");

                // Create the relationship
                var userPlant = new UserPlant
                {
                    UserId = userId,
                    PlantId = plant.Id
                };

                _context.UserPlants.Add(userPlant);
                await _context.SaveChangesAsync();

                // Return only the relevant data
                var plantDto = new PlantDto
                {
                    Id = plant.Id,
                    PermapeopleId = plant.PermapeopleId,
                    Name = plant.Name,
                    ImageUrl = plant.ImageUrl,
                    WaterRequirement = plant.WaterRequirement,
                    LightRequirement = plant.LightRequirement
                };

                return Ok(new
                {
                    message = "Plant added to your garden",
                    plant = plantDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding plant to user's garden");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        // remove a plant from the user's garden
        [HttpDelete("{plantId}")]
        public async Task<IActionResult> RemovePlantFromGarden(int plantId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                ?? throw new UnauthorizedAccessException());

            try
            {
                var userPlant = await _context.UserPlants
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PlantId == plantId);

                if (userPlant == null)
                    return NotFound("This plant is not in your garden.");

                _context.UserPlants.Remove(userPlant);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Plant removed from your garden" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing plant {PlantId} for user {UserId}", plantId, userId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPost("water")]
        public async Task<IActionResult> WaterPlant([FromBody] WaterPlantDto waterPlantDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                ?? throw new UnauthorizedAccessException());

            try
            {
                // Get the user's plant with plant details
                var userPlant = await _context.UserPlants
                    .Include(up => up.Plant)
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PlantId == waterPlantDto.PlantId);

                if (userPlant == null)
                    return NotFound("Plant not found in your garden");

                // Water the plant
                var today = DateOnly.FromDateTime(DateTime.Today);
                userPlant.LastWatered = today;
                userPlant.NextWatering = CalculateNextWateringDate(userPlant, today);

                await _context.SaveChangesAsync();

                return Ok(new 
                {
                    message = "Plant watered successfully!",
                    lastWatered = userPlant.LastWatered,
                    nextWatering = userPlant.NextWatering,
                    waterRequirement = userPlant.Plant.WaterRequirement
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error watering plant {PlantId} for user {UserId}", 
                    waterPlantDto.PlantId, userId);
                return StatusCode(500, "An error occurred while watering your plant");
            }
        }

        private DateOnly CalculateNextWateringDate(UserPlant userPlant, DateOnly wateringDate)
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

    public class PlantApiResponse
    {
        public int Id { get; set; } // (Actually the permapeople id)
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string WaterRequirement { get; set; } = string.Empty;
        public string LightRequirement { get; set; } = string.Empty;
    }

    public class WaterPlantDto
    {
        public int PlantId { get; set; }
    }
}
