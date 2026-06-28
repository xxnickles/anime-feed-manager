namespace AnimeFeedManager.Shared;

/// <summary>
/// Custom <see cref="System.Diagnostics.ActivitySource"/> names for OpenTelemetry tracing.
/// Sources are registered via <c>tracing.AddSource(...)</c> — passed into
/// <c>AddWebAppDefaults(...)</c> at the Web composition root. Add new names here AND to that
/// call so the spans are sampled.
/// </summary>
public static class Telemetry
{
    /// <summary>
    /// Source for library-import orchestrator activities (<c>Library.Import</c>,
    /// <c>Library.Import.Page</c>). Carries input counts, succeeded/failed counts,
    /// and per-page RU cost aggregates.
    /// </summary>
    public const string LibraryImportSource = "AnimeFeedManager.Library.Import";

    /// <summary>
    /// Source for page-serving catalog reads (series by season / by id, seasons-index reads).
    /// Leaf storage spans carry the query keys, result counts, and RU cost.
    /// </summary>
    public const string LibraryCatalogSource = "AnimeFeedManager.Library.Catalog";

    /// <summary>
    /// Source for authentication storage activities (account read/upsert, users-index read/registration).
    /// Leaf storage spans carry RU cost and ETag retry counts.
    /// </summary>
    public const string AuthSource = "AnimeFeedManager.Auth";
}
