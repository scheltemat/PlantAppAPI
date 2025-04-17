namespace PlantAppServer.Models;

// Join Entity
public class UserPlant
{
    public int UserId { get; set; }      // Matches ApplicationUser's Id type (int)
    public int PlantId { get; set; }     // Matches Plant's Id type
    // adding the following properties to the UserPlant model
    public DateOnly? LastWatered { get; set; } // Date when the plant was last watered
    public DateOnly? NextWatering { get; set; } // Date when the plant needs to be watered next
    
    // Navigation properties
    public ApplicationUser User { get; set; }
    public Plant Plant { get; set; }
}