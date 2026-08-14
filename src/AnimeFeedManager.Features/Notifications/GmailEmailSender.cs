using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AnimeFeedManager.Features.Notifications;

/// <summary>
/// Sends a pre-rendered HTML email. Template rendering is a separate concern (the Web layer's
/// Blazor renderer) — this delegate only knows about transport.
/// </summary>
public delegate Task<Result<Unit>> EmailSender(
    string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

public static class GmailEmailSender
{
    private const string GmailSmtpHost = "smtp.gmail.com";
    private const int GmailSmtpPort = 587;

    public static EmailSender GmailEmailSenderHandler(this IOptions<GmailOptions> options) =>
        (to, subject, htmlBody, cancellationToken) => Send(options.Value, to, subject, htmlBody, cancellationToken);

    private static async Task<Result<Unit>> Send(
        GmailOptions options, string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(options.FromName, options.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(GmailSmtpHost, GmailSmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(options.FromEmail, options.AppPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            return new Unit();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // Recipient address never rides in the error/log — PII stays out of traces. Callers
            // that need to correlate a failure use UserId, not the raw email.
            return ExceptionError.FromException(e);
        }
    }
}
