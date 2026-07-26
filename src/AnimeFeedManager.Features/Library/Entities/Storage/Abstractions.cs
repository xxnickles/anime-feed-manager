namespace AnimeFeedManager.Features.Library.Entities.Storage;

/// <summary>Write-only — one document per persisted Library occurrence.</summary>
public delegate Task<Result<Unit>> LibraryEventUpserter(LibraryEvent libraryEvent, CancellationToken cancellationToken);
