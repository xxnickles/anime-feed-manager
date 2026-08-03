namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// Intermediate base for every series-scoped document in the <c>feeds</c> container —
/// classification, confirmation markers, release history, and subscribers all share a
/// partition per <see cref="SeriesId"/>. Source-scoped documents (checkpoint, run, occurrence)
/// stay directly on <see cref="FeedsDocument"/>: they aggregate across many series in one
/// document, so there's no single series to key them by.
/// </summary>
public abstract record SeriesFeedsDocument : FeedsDocument
{
    public int SeriesId { get; }

    protected SeriesFeedsDocument(int seriesId)
    {
        SeriesId = seriesId;
        PartitionKey = seriesId.ToString();
    }
}
