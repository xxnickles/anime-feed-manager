namespace AnimeFeedManager.Shared;

/// <summary>
/// Custom <see cref="System.Diagnostics.ActivitySource"/> names for OpenTelemetry tracing.
/// ServiceDefaults registers these names via <c>tracing.AddSource(...)</c> in
/// <c>ConfigureOpenTelemetry</c> — update both locations when adding or renaming sources.
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
    /// Source for cover-image processing activities (<c>Library.Image.Process</c>). The import
    /// enqueues image work onto a separate queue, so each activity is started with the import's
    /// captured <see cref="System.Diagnostics.ActivityContext"/> as parent — nesting the
    /// download/upload/patch under the import's trace across the queue boundary.
    /// </summary>
    public const string LibraryImageSource = "AnimeFeedManager.Library.Image";
}
