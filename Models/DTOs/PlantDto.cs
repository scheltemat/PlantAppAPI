namespace PlantAppServer.Models.DTOs
{
    public class PlantDto
    {
        public int Id { get; set; }
        public int PermapeopleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string WaterRequirement { get; set; } = string.Empty;
        public string LightRequirement { get; set; } = string.Empty;
        public DateOnly? LastWatered { get; set; } 
        public DateOnly? NextWatering { get; set; } 
        public bool NeedsWatering { get; set; } 
    }
}
