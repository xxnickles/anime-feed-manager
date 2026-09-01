namespace AnimeFeedManager.Shared;

/// <summary>
/// Custom <see cref="System.Diagnostics.ActivitySource"/> names for OpenTelemetry tracing.
/// Sources are registered via <c>tracing.AddSource(...)</c> — passed into
/// <c>AddServiceDefaults(...)</c> at each host's composition root. Add new names here AND to
/// that call so the spans are sampled.
/// </summary>
public static class Telemetry
{
    /// <summary>Season creation/update orchestration: existing-season check, create-or-update, demote previous latest.</summary>
    public const string SeasonsUpdateSource = "AnimeFeedManager.Seasons.Update";

    /// <summary>Primary TV library scrape/import orchestration for a season.</summary>
    public const string TvLibraryImportSource = "AnimeFeedManager.Tv.Library.Import";

    /// <summary>Marking ongoing series as completed once their feed stops producing new episodes.</summary>
    public const string TvLibraryCompletionSource = "AnimeFeedManager.Tv.Library.Completion";

    /// <summary>Feed-titles scrape-and-store pipeline: fetch feed titles, match against the library, persist.</summary>
    public const string TvLibraryFeedTitlesSource = "AnimeFeedManager.Tv.Library.FeedTitles";

    /// <summary>Library catalog queries (series lookups, season indexes).</summary>
    public const string TvLibraryQueriesSource = "AnimeFeedManager.Tv.Library.Queries";

    /// <summary>Daily feed processing and per-user notification dispatch.</summary>
    public const string TvSubscriptionsFeedSource = "AnimeFeedManager.Tv.Subscriptions.Feed";

    /// <summary>Auto-subscription management for interested users on a series.</summary>
    public const string TvSubscriptionsManagementSource = "AnimeFeedManager.Tv.Subscriptions.Management";

    /// <summary>System event update/dispatch orchestration.</summary>
    public const string SystemEventsUpdateSource = "AnimeFeedManager.SystemEvents.Update";

    /// <summary>User authentication: credential/user registration and login verification.</summary>
    public const string UserAuthenticationSource = "AnimeFeedManager.User.Authentication";

    /// <summary>Series image provisioning.</summary>
    public const string ImagesSource = "AnimeFeedManager.Images";

    /// <summary>Web admin endpoint handlers.</summary>
    public const string WebAdminSource = "AnimeFeedManager.Web.Admin";

    /// <summary>Web security (auth/credential) endpoint handlers.</summary>
    public const string WebSecuritySource = "AnimeFeedManager.Web.Security";

    /// <summary>Web TV library/subscription endpoint handlers.</summary>
    public const string WebTvSource = "AnimeFeedManager.Web.Tv";
}
