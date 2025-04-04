using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantAppServer.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PlantAppServer.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
      return await _context.Users.ToListAsync();
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
      var user = await _context.Users.FindAsync(id);

      if (user == null)
      {
          return NotFound();
      }

      return user;
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
      // Check if a user already exists with the same email or username
      var existingUser = await _context.Users
          .FirstOrDefaultAsync(u => u.Email == user.Email || u.Username == user.Username);

      if (existingUser != null)
      {
        // If a user with the same email or username exists, return a conflict status (409)
        return Conflict(new { message = "User with the same email or username already exists." });
      }
      // Hash the user's password before saving it
      user.Password = HashPassword(user.Password);

      _context.Users.Add(user);
      await _context.SaveChangesAsync();
      
      return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
      if (id != user.Id)
      {
        return BadRequest();
      }

      // If the password is provided, hash it
      if (!string.IsNullOrEmpty(user.Password))
      {
        user.Password = HashPassword(user.Password);
      }

      _context.Entry(user).State = EntityState.Modified;

      try
      {
          await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!UserExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
      var user = await _context.Users.FindAsync(id);
      if (user == null)
      {
        return NotFound();
      }

      _context.Users.Remove(user);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool UserExists(int id)
    {
      return _context.Users.Any(e => e.Id == id);
    }

    // Helper method to hash passwords
    private string HashPassword(string password)
    {
      // Generate a salt (unique per password)
      byte[] salt = new byte[128 / 8]; // 128-bit salt
      using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
      {
        rng.GetBytes(salt);
      }

      // Hash the password with the salt using PBKDF2
      string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: password,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));

      return $"{Convert.ToBase64String(salt)}.{hashedPassword}"; // Store salt and hashed password
    }

    // Helper method to verify passwords
    private bool VerifyPassword(string enteredPassword, string storedPassword)
    {
      var parts = storedPassword.Split('.');
      if (parts.Length != 2) return false;

      byte[] salt = Convert.FromBase64String(parts[0]);
      string storedHashedPassword = parts[1];

      // Hash the entered password with the stored salt
      string hashedEnteredPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: enteredPassword,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));

      return hashedEnteredPassword == storedHashedPassword;
    }
  }
}
