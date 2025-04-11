using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantAppServer.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/external/plants")]
public class PermapeopleController : ControllerBase
{
    private readonly IPermapeopleService _permapeopleService;
    private readonly ILogger<PermapeopleController> _logger;

    public PermapeopleController(
        IPermapeopleService permapeopleService,
        ILogger<PermapeopleController> logger)
    {
        _permapeopleService = permapeopleService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("{permapeopleId}")]
    public async Task<IActionResult> GetPlant(int permapeopleId)
    {
        try
        {
            var plant = await _permapeopleService.GetPlantByIdAsync(permapeopleId);
            if (plant == null)
            {
                _logger.LogInformation("Plant with ID {Id} not found", permapeopleId);
                return NotFound();
            }
            return Ok(plant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plant with ID {Id}", permapeopleId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> SearchPlants(string query) 
    {
        try
        {
            var plants = await _permapeopleService.SearchPlantsAsync(query);
            if (plants == null)
            {
                _logger.LogInformation("Invalid query for: " + query);
                return NotFound();
            }
            return Ok(plants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plant query");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}