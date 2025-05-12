using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Doner.Features.WorkspaceFeature.Services.EmailService;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class EmailService(IOptions<SmtpSettings> config) : IEmailService
{
    private readonly SmtpSettings _settings = config.Value;

    public async Task SendEmailInviteAsync(string toEmail, string username, string link)
    {
        var subject = "You're invited to join a workspace";
        var plainTextContent = $"Dear {username},\n\nYou've been invited to join a workspace.\nPlease click the link below to accept the invitation:\n{link}\n\nBest regards,\nYour Team";
        var htmlContent = $"<p>Dear {username},</p><p>You've been invited to join a workspace.</p><p><a href='{link}'>Click here to accept</a></p><br><p>Best regards,<br>Your Team</p>";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlContent,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(toEmail, username));

        using var smtp = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = true
        };

        try
        {
            await smtp.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to send email via SMTP", ex);
        }
    }
}