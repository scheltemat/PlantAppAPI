using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Define DbSets (tables) here to add models
    // public DbSet<YourModel> YourModels { get; set; }
}
