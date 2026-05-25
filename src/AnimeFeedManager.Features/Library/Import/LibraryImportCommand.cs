namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Work-queue command requesting a library import for the supplied
/// <see cref="ImportTarget"/>.
/// </summary>
public sealed record LibraryImportCommand(ImportTarget Target);
