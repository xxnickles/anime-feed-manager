using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Entities.Storage;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Persists <see cref="OperationFailed"/> occurrences raised by Library's own jobs (currently just
/// import) as <see cref="LibraryEvent"/> documents. <see cref="OperationFailed"/> is a shared,
/// cross-cutting signal — the <see cref="LibrarySources.Import"/> check is the filter that keeps
/// Library only persisting the sources it owns, ignoring every other feature's failures.
/// </summary>
internal sealed class LibraryEventHandler(
    ICosmosContainerFactory cosmosFactory,
    ILogger<LibraryEventHandler> logger) : EventSubscriber<OperationFailed>
{
    private readonly LibraryEventUpserter _upsertEvent = cosmosFactory.CosmosLibraryEventUpserterHandler();

    public override Task Handle(OperationFailed evt, CancellationToken cancellationToken)
    {
        if (evt.Source != LibrarySources.Import) return Task.CompletedTask;

        var libraryEvent = new LibraryEvent(evt.Source)
        {
            Kind = "import-failed",
            Outcome = evt.Outcome,
            Summary = evt.Message,
            OccurredAt = evt.OccurredAt
        };

        return _upsertEvent(libraryEvent, cancellationToken)
            .AddLogOnFailure(_ => log => log.LogWarning(
                "Failed to persist library event for source {Source}", evt.Source))
            .Complete(logger);
    }
}
