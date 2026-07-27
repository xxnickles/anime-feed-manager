namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Stable source identifiers for Library's <c>LibraryEvent</c> persistence — shared by both
/// <c>OperationFailed</c> (import failures) and <c>SeasonImported</c> (import successes), since
/// <c>RecentLibraryEventsLoader</c> partitions its query by this value.
/// </summary>
internal static class LibrarySources
{
    public const string Import = "LibraryImport";
}
