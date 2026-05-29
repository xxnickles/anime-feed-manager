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
}
