using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Matching;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Outcome of reconciling a matched Nyaa entry against what's already confirmed for that
/// series — closed so consumers pattern-match instead of branching on nulls. <see cref="NewRelease"/>
/// is itself closed by episode shape (single/batch/none), mirroring <see cref="ReleaseContent"/>,
/// so building a <see cref="ReleaseDetected"/> never has to guess which of Episode/EpisodeRangeEnd
/// actually applies.
/// </summary>
internal abstract record ReconciliationResult
{
    private ReconciliationResult()
    {
    }

    public abstract record NewRelease : ReconciliationResult
    {
        private NewRelease()
        {
        }

        public abstract NyaaConfirmation UpdatedConfirmation { get; init; }
        public abstract ReleaseContentType ContentType { get; init; }

        public sealed record SingleEpisode(NyaaConfirmation UpdatedConfirmation, ReleaseContentType ContentType, int Episode) : NewRelease;

        public sealed record BatchRelease(NyaaConfirmation UpdatedConfirmation, ReleaseContentType ContentType, int EpisodeStart, int EpisodeEnd) : NewRelease;

        public sealed record WithoutEpisodeCount(NyaaConfirmation UpdatedConfirmation, ReleaseContentType ContentType) : NewRelease;
    }

    public sealed record AlreadyConfirmed : ReconciliationResult;
}

/// <summary>
/// Decides whether a matched Nyaa entry is genuinely new, given what's already confirmed for
/// that series. Episodic/batch content only counts as new once it advances past
/// <see cref="NyaaConfirmation.LastConfirmedEpisode"/>; non-numbered content (movie/OVA) counts
/// as new only the first time (the confirmation document's mere existence is the marker).
/// BD/remux releases bypass the advancement gate entirely — a BD reissue is always notable
/// (quality-upgrade/collector case) even when the same episodes already aired on the web, but
/// still never regresses the confirmation's high-water mark.
/// </summary>
internal static class NyaaCollectionReconciler
{
    public static ReconciliationResult Reconcile(MatchedRelease release, NyaaConfirmation? previous)
    {
        if (release.IsBdRemux)
            return NewReleaseFor(release, ReleaseContentType.BdRemux, previous);

        return release.Content switch
        {
            ReleaseContent.SingleEpisode(var number) => IsAdvancing(number, previous?.LastConfirmedEpisode)
                ? NewReleaseFor(release, ReleaseContentType.Episode, previous)
                : new ReconciliationResult.AlreadyConfirmed(),

            ReleaseContent.Batch(_, var end) => IsAdvancing(end, previous?.LastConfirmedEpisode)
                ? NewReleaseFor(release, ReleaseContentType.Batch, previous)
                : new ReconciliationResult.AlreadyConfirmed(),

            ReleaseContent.NonNumbered => previous is null
                ? NewReleaseFor(release, ReleaseContentType.MovieOrOva, previous)
                : new ReconciliationResult.AlreadyConfirmed(),

            _ => new ReconciliationResult.AlreadyConfirmed()
        };
    }

    private static bool IsAdvancing(int candidateEpisode, int? previousEpisode) =>
        previousEpisode is null || candidateEpisode > previousEpisode.Value;

    private static ReconciliationResult.NewRelease NewReleaseFor(
        MatchedRelease release, ReleaseContentType contentType, NyaaConfirmation? previous)
    {
        var confirmation = (previous ?? new NyaaConfirmation(release.SeriesId))
            with { LastConfirmedEpisode = NextConfirmedEpisode(release.Content, previous?.LastConfirmedEpisode) };

        return release.Content switch
        {
            ReleaseContent.SingleEpisode(var number) =>
                new ReconciliationResult.NewRelease.SingleEpisode(confirmation, contentType, number),

            ReleaseContent.Batch(var start, var end) =>
                new ReconciliationResult.NewRelease.BatchRelease(confirmation, contentType, start, end),

            _ => new ReconciliationResult.NewRelease.WithoutEpisodeCount(confirmation, contentType)
        };
    }

    // Never regresses the high-water mark — a BD reissue of already-confirmed episodes still
    // bypasses the notification gate above, but the stored confirmation stays at whichever is higher.
    private static int? NextConfirmedEpisode(ReleaseContent content, int? previousEpisode) => content switch
    {
        ReleaseContent.SingleEpisode(var number) => MaxOf(number, previousEpisode),
        ReleaseContent.Batch(_, var end) => MaxOf(end, previousEpisode),
        _ => previousEpisode
    };

    private static int MaxOf(int candidate, int? previous) => previous is null ? candidate : Math.Max(candidate, previous.Value);
}
