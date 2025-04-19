using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PlantAppServer.Models;

namespace PlantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;  // Inject JwtSettings

        public AuthController(UserManager<ApplicationUser> userManager, 
                              SignInManager<ApplicationUser> signInManager,
                              IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;  // Retrieve JwtSettings
        }

        // Register user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Password and Confirm Password do not match.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // You might want to sign in the user, but since we're using JWTs, we'll skip this.
                    return Ok(new { message = "Registration successful" });
                }

                // Return validation errors
                return BadRequest(result.Errors);
            }

            return BadRequest("Invalid registration details");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    // Check the password against the user
                    var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (isValid)
                    {
                        // Generate the JWT token after successful login
                        var token = GenerateJwtToken(user);

                        // Return the token in the response
                        return Ok(new { token });
                    }
                    else
                    {
                        return Unauthorized("Invalid credentials.");
                    }
                }
                else
                {
                    return Unauthorized("User not found.");
                }
            }

            return BadRequest("Invalid data.");
        }


        [Authorize]
        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Try multiple ways to get the user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            // Debugging: Log all claims
            Console.WriteLine("All claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user ID format.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            return Ok(new UserModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // No need to manage cookies; JWT handles this on the client-side (no logout on server-side needed).
            return  Ok(new { message = "Logged out successfully" });
        }

        // Helper method to generate JWT token
        public string GenerateJwtToken(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(_jwtSettings.SecretKey))
            {
                throw new ArgumentNullException("SecretKey cannot be null or empty.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (user ID)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
                new Claim(JwtRegisteredClaimNames.Email, user.Email), // Email
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // NameIdentifier (user ID)
                new Claim(ClaimTypes.Name, user.UserName) // Username
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterModel
    {
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class UserModel
    {
        // Id is now of type int to match ApplicationUser's Id
        public int Id { get; set; }  
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}
