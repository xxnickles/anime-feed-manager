using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Entities.Storage;
using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Persists a successful <see cref="SeasonImported"/> as a <see cref="LibraryEvent"/> so it shows
/// up in the admin activity feed alongside import failures, not just as a transient SSE toast.
/// </summary>
internal sealed class SeasonImportedEventHandler(
    ICosmosContainerFactory cosmosFactory,
    ILogger<SeasonImportedEventHandler> logger) : EventSubscriber<SeasonImported>
{
    private readonly LibraryEventUpserter _upsertEvent = cosmosFactory.CosmosLibraryEventUpserterHandler();

    public override Task Handle(SeasonImported evt, CancellationToken cancellationToken)
    {
        var libraryEvent = new LibraryEvent(LibrarySources.Import)
        {
            Kind = "import-succeeded",
            Outcome = Outcome.Success,
            Summary = evt.ToSummary(),
            OccurredAt = evt.OccurredAt
        };

        return _upsertEvent(libraryEvent, cancellationToken)
            .AddLogOnFailure(_ => log => log.LogWarning(
                "Failed to persist library event for source {Source}", LibrarySources.Import))
            .Complete(logger);
    }
}
