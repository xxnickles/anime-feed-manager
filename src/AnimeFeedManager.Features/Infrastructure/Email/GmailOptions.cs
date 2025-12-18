namespace AnimeFeedManager.Features.Infrastructure.Email;

/// <summary>
/// Gmail SMTP configuration options
/// </summary>
public sealed class GmailOptions
{
    public const string SectionName = "Gmail";

    /// <summary>
    /// Sender email address (Gmail)
    /// </summary>
    public string FromEmail { get; init; } = string.Empty;

    /// <summary>
    /// Sender display name
    /// </summary>
    public string FromName { get; init; } = string.Empty;

    /// <summary>
    /// Gmail app password (not regular password)
    /// </summary>
    public string AppPassword { get; init; } = string.Empty;
}
