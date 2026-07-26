namespace AnimeFeedManager.Shared.Types;

/// <summary>
/// Uniform shape for a significant domain occurrence persisted for later insights (graphs,
/// error drill-in). Each feature implements this on its own Cosmos document type rather than
/// sharing a container — see <c>LibraryEvent</c> for the first concrete example.
/// </summary>
public interface IPersistedEvent
{
    DateTimeOffset OccurredAt { get; }
    string Source { get; }
    string Kind { get; }
    Outcome Outcome { get; }
    string Summary { get; }
}
