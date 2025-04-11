using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlantAppServer.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for your entities
    public DbSet<Plant> Plants { get; set; }
    public DbSet<UserPlant> UserPlants { get; set; } // Join table

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure the many-to-many relationship
        builder.Entity<UserPlant>()
            .HasKey(up => new { up.UserId, up.PlantId }); // Composite primary key

        builder.Entity<UserPlant>()
            .HasOne(up => up.User)
            .WithMany(u => u.UserPlants)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserPlant>()
            .HasOne(up => up.Plant)
            .WithMany(p => p.UserPlants)
            .HasForeignKey(up => up.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}