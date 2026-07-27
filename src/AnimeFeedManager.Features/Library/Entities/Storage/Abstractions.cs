namespace AnimeFeedManager.Features.Library.Entities.Storage;

/// <summary>Write-only — one document per persisted Library occurrence.</summary>
public delegate Task<Result<Unit>> LibraryEventUpserter(LibraryEvent libraryEvent, CancellationToken cancellationToken);

/// <summary>
/// Most recent <paramref name="take"/> Library events, newest first. Scoped internally to
/// Library's own known source(s) — callers don't need to know the source-naming scheme.
/// </summary>
public delegate Task<Result<ImmutableArray<LibraryEvent>>> RecentLibraryEventsLoader(
    int take, CancellationToken cancellationToken);
