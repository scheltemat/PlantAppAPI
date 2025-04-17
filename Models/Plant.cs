namespace PlantAppServer.Models;
using System.Collections.Generic;

public class Plant
{
    public int Id { get; set; }
    public int PermapeopleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    // adding the following properties to the Plant model
    public string WaterRequirement { get; set; } = string.Empty;
    public string LightRequirement { get; set; } = string.Empty;
    
    // Navigation property for the many-to-many relationship
    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
    
}