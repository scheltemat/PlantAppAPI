using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantAppServer.Models.DTOs;

namespace PlantAppServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all users
        [HttpGet("")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserPlants) // Include the user's plants
                .ThenInclude(up => up.Plant) // Include the plant details
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No users found.");
            }

            // Convert users to DTOs
            var userDtos = users.Select(user => new UserWithPlantsDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                // Only include userPlants if there are any
                UserPlants = user.UserPlants.Any()
                    ? user.UserPlants.Select(up => new PlantDto
                    {
                        Id = up.Plant.Id,
                        PermapeopleId = up.Plant.PermapeopleId,
                        Name = up.Plant.Name,
                        ImageUrl = up.Plant.ImageUrl
                    }).ToList()
                    : null // Set to null if no plants exist for the user
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserPlants)
                .ThenInclude(up => up.Plant)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var userDto = new UserWithPlantsDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                UserPlants = user.UserPlants.Select(up => new PlantDto
                {
                    Id = up.Plant.Id,
                    PermapeopleId = up.Plant.PermapeopleId,
                    Name = up.Plant.Name,
                    ImageUrl = up.Plant.ImageUrl
                }).ToList()
            };

            return Ok(userDto);
        }
    }
}
