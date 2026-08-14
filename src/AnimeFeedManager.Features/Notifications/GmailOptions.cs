namespace AnimeFeedManager.Features.Notifications;

/// <summary>
/// Configuration for the Gmail SMTP sender. Bind from configuration section <see cref="SectionName"/>.
/// <see cref="AppPassword"/> is a Gmail app password, not the account password.
/// </summary>
public sealed class GmailOptions
{
    public const string SectionName = "Gmail";

    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
}
