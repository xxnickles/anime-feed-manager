namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// Idempotency marker for the Nyaa collection job — one document per series once it has at
/// least one confirmed release, partitioned by <see cref="SeriesId"/>. Written only by that job.
/// <see cref="LastConfirmedEpisode"/> is null for single-release formats (movie/OVA), where the
/// document's mere existence is the confirmation; for episodic formats it holds the highest
/// episode number confirmed so far (a batch release advances it to the batch's end).
/// </summary>
public sealed record NyaaConfirmation : FeedsDocument
{
    public int SeriesId { get; }

    public int? LastConfirmedEpisode { get; init; }

    public NyaaConfirmation(int seriesId)
    {
        SeriesId = seriesId;
        Id = $"nyaa-confirmation:{seriesId}";
        PartitionKey = seriesId.ToString();
    }
}
