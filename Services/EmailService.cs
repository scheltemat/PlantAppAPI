using System.Net;
using System.Net.Mail;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class SmtpEmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SmtpEmailService()
    {
        _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? throw new Exception("Missing SMTP_HOST");
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _smtpUser = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? throw new Exception("Missing SMTP_USERNAME");
        _smtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? throw new Exception("Missing SMTP_PASSWORD");
        _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? throw new Exception("Missing SMTP_FROM_EMAIL");
        _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Plant Reminder Bot";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var mail = new MailMessage();
        mail.From = new MailAddress(_fromEmail, _fromName);
        mail.To.Add(to);
        mail.Subject = subject;
        mail.Body = body;

        using var smtp = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUser, _smtpPass),
            EnableSsl = true
        };

        await smtp.SendMailAsync(mail);
    }
}
