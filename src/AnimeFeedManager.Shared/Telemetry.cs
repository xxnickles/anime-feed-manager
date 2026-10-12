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

    /// <summary>System event update/dispatch orchestration.</summary>
    public const string SystemEventsUpdateSource = "AnimeFeedManager.SystemEvents.Update";

    /// <summary>Queue-message processing in the Functions host: links a handler span to the producer's trace.</summary>
    public const string FunctionsMessageProcessingSource = "AnimeFeedManager.Functions.MessageProcessing";

    /// <summary>Queue send spans; both hosts produce messages.</summary>
    public const string MessagingSource = "AnimeFeedManager.Messaging";

    /// <summary>Series image provisioning.</summary>
    public const string ImagesSource = "AnimeFeedManager.Images";

    /// <summary>Web admin endpoint handlers.</summary>
    public const string WebAdminSource = "AnimeFeedManager.Web.Admin";

    /// <summary>Web security (auth/credential) endpoint handlers.</summary>
    public const string WebSecuritySource = "AnimeFeedManager.Web.Security";

    /// <summary>Web TV library/subscription endpoint handlers.</summary>
    public const string WebTvSource = "AnimeFeedManager.Web.Tv";

    // Cross-feature keys, shared by span tags and log properties. Feature keys live in that feature's *Telemetry class.

    public const string SeriesIdTag = "series.id";

    /// <summary>Partition-key format, e.g. <c>2026-fall</c>.</summary>
    public const string SeriesSeasonTag = "series.season";

    public const string SeriesTitleTag = "series.title";

    /// <summary>Opaque user id; never the email.</summary>
    public const string UserIdTag = "user.id";

    /// <summary>String array, per semconv.</summary>
    public const string UserRolesTag = "user.roles";

    /// <summary>Log-only: carries the operation to backends that never see the span name.</summary>
    public const string OperationNameTag = "operation.name";

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

    /// <summary>Custom value: semconv registers none for Azure Storage Queues.</summary>
    public const string MessagingSystemValue = "azure_storage_queue";

    /// <summary>Both <c>messaging.operation.name</c> and <c>.type</c>; the send span is named <c>send {queue}</c>.</summary>
    public const string MessagingSendOperation = "send";
}
