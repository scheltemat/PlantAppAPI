namespace PlantAppServer.Models.DTOs
{
    public class PlantDto
    {
        public int Id { get; set; }
        public int PermapeopleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
