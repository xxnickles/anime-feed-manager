using AnimeFeedManager.Features.Infrastructure.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AnimeFeedManager.Web.BlazorComponents.Email;

/// <summary>
/// Rendered multipart/alternative body
/// </summary>
internal sealed record EmailBody(string Html, string Text);

/// <summary>
/// Email sender implementation using Gmail SMTP with Blazor component rendering
/// </summary>
public sealed class EmailNotificationSender : IEmailNotificationSender
{
    private const string GmailSmtpHost = "smtp.gmail.com";
    private const int GmailSmtpPort = 587;

    private readonly GmailOptions _options;
    private readonly BlazorRenderer _blazorRenderer;

    public EmailNotificationSender(
        IOptions<GmailOptions> options,
        BlazorRenderer blazorRenderer)
    {
        _options = options.Value;
        _blazorRenderer = blazorRenderer;
    }

    public Task<Result<Unit>> Send<TModel>(
        string to,
        string subject,
        EmailTemplateFactory<TModel> templateFactory,
        EmailTextFactory<TModel> textFactory,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : notnull
    {
        try
        {
            return RenderBody(templateFactory, textFactory, model)
                .Bind(body => SendEmailAsync(to, subject, body, cancellationToken));
        }
        catch (Exception ex)
        {
            return EmailSendError.Create($"Unexpected error: {ex.Message}")
                .AsTaskFailure<Unit>();
        }
    }

    private async Task<Result<EmailBody>> RenderBody<TModel>(
        EmailTemplateFactory<TModel> templateFactory,
        EmailTextFactory<TModel> textFactory,
        TModel model) where TModel : notnull
    {
        try
        {
            var htmlContent = await _blazorRenderer.RenderComponent<WrapperComponent>(
                new Dictionary<string, object?>
                {
                    [nameof(WrapperComponent.ChildContent)] = templateFactory(model)
                });

            return new EmailBody(htmlContent, textFactory(model));
        }
        catch (Exception ex)
        {
            return EmailRenderError.Create($"Template rendering failed: {ex.Message}");
        }
    }

    private async Task<Result<Unit>> SendEmailAsync(
        string to,
        string subject,
        EmailBody body,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body.Html,
                TextBody = body.Text
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(GmailSmtpHost, GmailSmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_options.FromEmail, _options.AppPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            return new Unit();
        }
        catch (Exception ex)
        {
            return EmailSendError.Create($"SMTP send failed: {ex.Message}");
        }
    }
}
