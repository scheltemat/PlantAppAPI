using Microsoft.AspNetCore.Mvc;

namespace PlantAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWateringService _wateringService;
        private readonly IEmailService _emailService;
        private readonly IReminderService _reminderService;

        public RemindersController(ApplicationDbContext context,
            IWateringService wateringService,
            IEmailService emailService,
            IReminderService reminderService)
        {
            _context = context;
            _wateringService = wateringService;
            _emailService = emailService;
            _reminderService = reminderService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendReminders()
        {
            await _reminderService.SendRemindersAsync();
            return Ok(new { Message = "Reminders sent" });
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            await _emailService.SendEmailAsync("thomas.scheltema@gmail.com", "Test Email", "This is a test!");
            return Ok("Email sent.");
        }

        public class EmailReplyModel
        {
            public string From { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
        }
    }
}
