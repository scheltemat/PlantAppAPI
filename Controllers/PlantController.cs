using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlantAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlantController> _logger;

        public PlantController(ApplicationDbContext context, ILogger<PlantController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all plants
        [HttpGet("")]
        public async Task<IActionResult> GetAllPlants()
        {
            try
            {
                var plants = await _context.Plants.ToListAsync();

                if (plants == null || plants.Count == 0)
                {
                    return NotFound("No plants found.");
                }

                return Ok(plants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plants.");
                return StatusCode(500, "An error occurred while retrieving plants.");
            }
        }

        // Get a single plant by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlantById(int id)
        {
            try
            {
                var plant = await _context.Plants
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (plant == null)
                {
                    return NotFound("Plant not found.");
                }

                return Ok(plant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plant by ID.");
                return StatusCode(500, "An error occurred while retrieving the plant.");
            }
        }

        // Delete a plant
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlant(int id)
        {
            try
            {
                var plant = await _context.Plants.FindAsync(id);
                if (plant == null)
                {
                    return NotFound("Plant not found.");
                }

                _context.Plants.Remove(plant);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Plant deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plant.");
                return StatusCode(500, "An error occurred while deleting the plant.");
            }
        }
    }
}
