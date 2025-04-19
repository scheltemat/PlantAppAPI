using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantAppServer.Models;
using PlantAppServer.Models.DTOs;
using System.Security.Claims;
using PlantAppServer.Services;

namespace PlantAppServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user-plants")]
    public class UserPlantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserPlantController> _logger;
        private readonly IWateringService _wateringService;

        public UserPlantController(
            ApplicationDbContext context, 
            ILogger<UserPlantController> logger, 
            IWateringService wateringService)
        {
            _context = context;
            _logger = logger;
            _wateringService = wateringService;
            
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

            var result = await _wateringService.WaterPlantAsync(userId, waterPlantDto.PlantId);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(new 
            {
                message = "Plant watered successfully!",
                lastWatered = result.LastWatered,
                nextWatering = result.NextWatering,
                waterRequirement = result.WaterRequirement
            });
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
