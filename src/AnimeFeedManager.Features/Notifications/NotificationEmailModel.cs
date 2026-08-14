using AnimeFeedManager.Features.Feeds.Entities;

namespace AnimeFeedManager.Features.Notifications;

/// <summary>One "available on" pill in a release card — a streaming platform or the synthesized Nyaa search link.</summary>
public sealed record PlatformPillModel(string Name, string Url, string Background, string Text);

/// <summary>One release, ready for the email template — one card per release, not per series.</summary>
public sealed record ReleaseCardModel(
    string SeriesTitle,
    ReleaseContentType ContentType,
    int? Episode,
    int? EpisodeRangeEnd,
    bool Confirmed,
    ImmutableArray<PlatformPillModel> Platforms);

/// <summary>The full digest email's content for one recipient.</summary>
public sealed record NotificationDigestView(
    string RecipientName, DateTimeOffset DigestDate, ImmutableArray<ReleaseCardModel> Releases);

/// <summary>
/// Renders a digest to HTML — implemented in the Web layer (Blazor <c>HtmlRenderer</c>), since
/// this project has no ASP.NET Core Components reference. Feeds <see cref="EmailSender"/>.
/// </summary>
public delegate Task<Result<string>> NotificationEmailRenderer(
    NotificationDigestView model, CancellationToken cancellationToken = default);
