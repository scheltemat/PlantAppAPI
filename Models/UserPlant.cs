namespace PlantAppServer.Models;

// Join Entity
public class UserPlant
{
    public int UserId { get; set; }      // Matches ApplicationUser's Id type (int)
    public int PlantId { get; set; }     // Matches Plant's Id type
    
    // Navigation properties
    public ApplicationUser User { get; set; }
    public Plant Plant { get; set; }
}