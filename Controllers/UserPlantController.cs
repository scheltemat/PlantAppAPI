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
                        ImageUrl = up.Plant.ImageUrl
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
                        ImageUrl = plantApiResponse.ImageUrl
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
                    ImageUrl = plant.ImageUrl
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

    }

    public class PlantApiResponse
    {
        public int Id { get; set; } // (Actually the permapeople id)
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
