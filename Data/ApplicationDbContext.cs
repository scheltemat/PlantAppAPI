using Microsoft.EntityFrameworkCore;
using PlantAppServer.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Define DbSets (tables) here to add models
    // public DbSet<YourModel> YourModels { get; set; }
    public DbSet<User> Users { get; set; }
}
