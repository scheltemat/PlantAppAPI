using Microsoft.AspNetCore.Identity;
using PlantAppServer.Models;

public class ApplicationUser : IdentityUser<int>
{
    // No additional properties; this class inherits all the default properties from IdentityUser
    
    // Navigation property for the many-to-many relationship
    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
}
