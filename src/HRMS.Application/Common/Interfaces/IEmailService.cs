namespace HRMS.Application.Common.Interfaces;

public record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    string? Cc = null);

/// <summary>
/// Email sending abstraction. Implemented in Infrastructure (SMTP, SendGrid, etc.).
/// Swappable without touching application logic.
/// </summary>
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    Task SendTemplateAsync(string to, string templateName, object model, CancellationToken cancellationToken = default);
}
