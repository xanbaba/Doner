using System.Net;
using System.Net.Mail;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Doner.Features.WorkspaceFeature.Services.EmailService;

public class MailjetSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class EmailService(IOptions<MailjetSettings> config) : IEmailService
{
    private readonly MailjetSettings _settings = config.Value;

    public async Task SendEmailInviteAsync(string toEmail, string username, string link)
    {
        var client = new MailjetClient(_settings.ApiKey, _settings.ApiSecret);

        var request = new MailjetRequest
            {
                Resource = SendV31.Resource,
            }
            .Property(Send.Messages, new JArray {
                new JObject {
                    {
                        "From", new JObject {
                            { "Email", _settings.FromEmail },
                            { "Name", _settings.FromName }
                        }
                    },
                    {
                        "To", new JArray {
                            new JObject {
                                { "Email", toEmail },
                                { "Name", username }
                            }
                        }
                    },
                    { "Subject", "You're invited to join a workspace" },
                    { "TextPart", $"Dear {username},\n\nYou've been invited to join a workspace.\n{link}" },
                    { "HTMLPart", $"<p>Dear {username},</p><p>You've been invited to join a workspace.</p><p><a href='{link}'>Click here to accept</a></p>" }
                }
            });

        var response = await client.PostAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Mailjet failed: {response.StatusCode} - {response.GetErrorMessage()}");
        }
    }
}