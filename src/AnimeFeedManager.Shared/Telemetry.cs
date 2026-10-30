namespace AnimeFeedManager.Shared;

/// <summary>
/// Custom <see cref="System.Diagnostics.ActivitySource"/> names for OpenTelemetry tracing.
/// Sources must be passed to each host's defaults call (<c>AddWebAppDefaults(...)</c> in Web,
/// <c>AddServiceDefaults(...)</c> in Functions). Add new names here AND to that call — an unregistered
/// source has no listener, so <c>StartActivity</c> silently returns null and the spans never exist.
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

    /// <summary>Nyaa feed bindings: bind, backfill, completion.</summary>
    public const string TvFeedManagerSource = "AnimeFeedManager.Tv.FeedManager";

    /// <summary>Feed manager admin queries (season feed list).</summary>
    public const string TvFeedManagerQueriesSource = "AnimeFeedManager.Tv.FeedManager.Queries";

    /// <summary>System event update/dispatch orchestration.</summary>
    public const string SystemEventsUpdateSource = "AnimeFeedManager.SystemEvents.Update";

    /// <summary>Queue send and process spans; both hosts produce messages, Functions consumes them.</summary>
    public const string MessagingSource = "AnimeFeedManager.Messaging";

    /// <summary>Series image provisioning.</summary>
    public const string ImagesSource = "AnimeFeedManager.Images";

    /// <summary>Web admin endpoint handlers.</summary>
    public const string WebAdminSource = "AnimeFeedManager.Web.Admin";

    /// <summary>Web security (auth/credential) endpoint handlers.</summary>
    public const string WebSecuritySource = "AnimeFeedManager.Web.Security";

    /// <summary>Web TV library/subscription endpoint handlers.</summary>
    public const string WebTvSource = "AnimeFeedManager.Web.Tv";

    /// <summary>Web SignalR negotiation.</summary>
    public const string WebRealTimeSource = "AnimeFeedManager.Web.RealTime";

    // Cross-feature keys, shared by span tags and log properties. Feature keys live in that feature's *Telemetry class.

    public const string SeriesIdTag = "series.id";

    /// <summary>Partition-key format, e.g. <c>2026-fall</c>.</summary>
    public const string SeriesSeasonTag = "series.season";

    public const string SeriesTitleTag = "series.title";

    /// <summary>Opaque user id; never the email.</summary>
    public const string UserIdTag = "user.id";

    /// <summary>Comma-joined role names; semconv's string[] would log as its type name.</summary>
    public const string UserRolesTag = "user.roles";

    /// <summary>Log-only: carries the operation to backends that never see the span name.</summary>
    public const string OperationNameTag = "operation.name";

    /// <summary>Retried attempts before a read-modify-write landed; present only when above 0.</summary>
    public const string WriteRetriesTag = "table.write_retries";

    public const string DomainErrorMessageTag = "domain_error.message";
    public const string DomainErrorSubjectTag = "domain_error.subject";

    public const string ValidationTypeNameTag = "validation.type_name";
    public const string ValidationErrorCountTag = "validation.error_count";
    public const string ValidationFieldCountTag = "validation.field_count";
    public const string ValidationFieldTag = "validation.field";
    public const string ValidationMessagesTag = "validation.messages";

    // OpenTelemetry semantic conventions

    /// <summary>DomainError class name, set on failure only.</summary>
    public const string ErrorTypeTag = "error.type";

    public const string MessagingSystemTag = "messaging.system";
    public const string MessagingOperationNameTag = "messaging.operation.name";
    public const string MessagingOperationTypeTag = "messaging.operation.type";
    public const string MessagingDestinationNameTag = "messaging.destination.name";
    public const string MessagingMessageIdTag = "messaging.message.id";
    /// <summary>Queue endpoint host; App Insights fills <c>target</c> only when it is set.</summary>
    public const string ServerAddressTag = "server.address";

    /// <summary>Custom value: semconv registers none for Azure Storage Queues.</summary>
    public const string MessagingSystemValue = "azure_storage_queue";

    /// <summary>Both <c>messaging.operation.name</c> and <c>.type</c>; the send span is named <c>send {queue}</c>.</summary>
    public const string MessagingSendOperation = "send";

    /// <summary>Both <c>messaging.operation.name</c> and <c>.type</c>; the consumer span is named <c>process {queue}</c>.</summary>
    public const string MessagingProcessOperation = "process";
}
