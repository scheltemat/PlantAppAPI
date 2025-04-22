namespace PlantAppServer.Jobs
{
    public class ReminderScheduler : BackgroundService
    {
        private readonly IServiceProvider _services;

        public ReminderScheduler(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"[ReminderScheduler] Running SendReminders at {DateTime.Now}");

                try
                {
                    using var scope = _services.CreateScope();
                    var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                    await reminderService.SendRemindersAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ReminderScheduler] Error: {ex.Message}");
                }

                // Run every 30 seconds (for testing)
                // await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                // Run once a day at 5 PM
                var now = DateTime.Now;
                var nextRun = now.Date.AddHours(17); // Today at 5:00 PM

                if (now > nextRun)
                    nextRun = nextRun.AddDays(1); // If it's already past 5 PM, wait until tomorrow

                var delay = nextRun - now;

                Console.WriteLine($"[ReminderScheduler] Next run scheduled at {nextRun} ({delay.TotalMinutes} minutes from now)");

                await Task.Delay(delay, stoppingToken);

            }
        }
    }
}
