namespace PlantAppServer.Models.DTOs
{
    public class UserWithPlantsDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<PlantDto> UserPlants { get; set; } = new();
    }
}
