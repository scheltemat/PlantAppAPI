using Microsoft.EntityFrameworkCore;

public interface IReminderService
{
    Task SendRemindersAsync();
}

public class ReminderService : IReminderService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ReminderService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendRemindersAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var userPlants = await _context.UserPlants
            .Include(up => up.User)
            .Include(up => up.Plant)
            .Where(up => up.NextWatering <= today || up.NextWatering == null)
            .ToListAsync();

        foreach (var up in userPlants)
        {
            var subject = $"Reminder: Time to water {up.Plant.Name}";
            var body = $"""
                Hi {up.User.UserName},

                It's time to water your plant: {up.Plant.Name} 🌿

                Happy gardening! 🌱
                - Your Plant Reminder Bot
                """;

            await _emailService.SendEmailAsync(up.User.Email, subject, body);
        }

        Console.WriteLine($"[ReminderService] Sent {userPlants.Count} reminder(s) at {DateTime.Now}");
    }
}
