using HRMS.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending Email to {To}: {Subject}", message.To, message.Subject);
        return Task.CompletedTask;
    }

    public Task SendTemplateAsync(string to, string templateName, object model, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending Template Email {Template} to {To}", templateName, to);
        return Task.CompletedTask;
    }
}
