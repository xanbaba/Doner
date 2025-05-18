namespace Doner.Features.WorkspaceFeature.Services.EmailService;

public interface IEmailService
{
    public Task SendEmailInviteAsync(string toEmail, string username, string link, string inviterName);
}