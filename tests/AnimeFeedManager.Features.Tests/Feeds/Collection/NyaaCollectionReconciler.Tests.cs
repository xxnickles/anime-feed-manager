using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;

namespace AnimeFeedManager.Features.Tests.Feeds.Collection;

public class NyaaCollectionReconcilerTests
{
    #region Single episode

    [Fact]
    public void Should_Return_NewRelease_When_No_Previous_Confirmation_Exists()
    {
        var release = Release(21, new ReleaseContent.SingleEpisode(5));

        var result = NyaaCollectionReconciler.Reconcile(release, previous: null);

        var newRelease = Assert.IsType<ReconciliationResult.NewRelease.SingleEpisode>(result);
        Assert.Equal(5, newRelease.Episode);
        Assert.Equal(ReleaseContentType.Episode, newRelease.ContentType);
        Assert.Equal(21, newRelease.UpdatedConfirmation.SeriesId);
        Assert.Equal(5, newRelease.UpdatedConfirmation.LastConfirmedEpisode);
    }

    [Fact]
    public void Should_Return_NewRelease_When_Episode_Advances_Past_Previous_Confirmation()
    {
        var release = Release(21, new ReleaseContent.SingleEpisode(6));
        var previous = new NyaaConfirmation(21) { LastConfirmedEpisode = 5 };

        var result = NyaaCollectionReconciler.Reconcile(release, previous);

        var newRelease = Assert.IsType<ReconciliationResult.NewRelease.SingleEpisode>(result);
        Assert.Equal(6, newRelease.Episode);
        Assert.Equal(6, newRelease.UpdatedConfirmation.LastConfirmedEpisode);
    }

    [Fact]
    public void Should_Return_AlreadyConfirmed_When_Episode_Does_Not_Advance()
    {
        var release = Release(21, new ReleaseContent.SingleEpisode(5));
        var previous = new NyaaConfirmation(21) { LastConfirmedEpisode = 5 };

        var result = NyaaCollectionReconciler.Reconcile(release, previous);

        Assert.IsType<ReconciliationResult.AlreadyConfirmed>(result);
    }

    #endregion

    #region Batch

    [Fact]
    public void Should_Return_NewRelease_When_Batch_Is_First_Confirmation()
    {
        var release = Release(21, new ReleaseContent.Batch(1, 12));

        var result = NyaaCollectionReconciler.Reconcile(release, previous: null);

        var newRelease = Assert.IsType<ReconciliationResult.NewRelease.BatchRelease>(result);
        Assert.Equal(1, newRelease.EpisodeStart);
        Assert.Equal(12, newRelease.EpisodeEnd);
        Assert.Equal(ReleaseContentType.Batch, newRelease.ContentType);
        Assert.Equal(12, newRelease.UpdatedConfirmation.LastConfirmedEpisode);
    }

    [Fact]
    public void Should_Return_AlreadyConfirmed_When_Batch_End_Does_Not_Advance()
    {
        var release = Release(21, new ReleaseContent.Batch(1, 12));
        var previous = new NyaaConfirmation(21) { LastConfirmedEpisode = 12 };

        var result = NyaaCollectionReconciler.Reconcile(release, previous);

        Assert.IsType<ReconciliationResult.AlreadyConfirmed>(result);
    }

    #endregion

    #region Non-numbered (movie/OVA)

    [Fact]
    public void Should_Return_NewRelease_When_NonNumbered_Has_No_Previous_Confirmation()
    {
        var release = Release(21, new ReleaseContent.NonNumbered());

        var result = NyaaCollectionReconciler.Reconcile(release, previous: null);

        var newRelease = Assert.IsType<ReconciliationResult.NewRelease.WithoutEpisodeCount>(result);
        Assert.Equal(ReleaseContentType.MovieOrOva, newRelease.ContentType);
        Assert.Null(newRelease.UpdatedConfirmation.LastConfirmedEpisode);
    }

    [Fact]
    public void Should_Return_AlreadyConfirmed_When_NonNumbered_Already_Confirmed_Once()
    {
        var release = Release(21, new ReleaseContent.NonNumbered());
        var previous = new NyaaConfirmation(21);

        var result = NyaaCollectionReconciler.Reconcile(release, previous);

        Assert.IsType<ReconciliationResult.AlreadyConfirmed>(result);
    }

    #endregion

    #region BD/remux bypass

    [Fact]
    public void Should_Return_NewRelease_When_BdRemux_Repeats_Already_Confirmed_Episodes()
    {
        var release = Release(21, new ReleaseContent.Batch(1, 12), isBdRemux: true);
        var previous = new NyaaConfirmation(21) { LastConfirmedEpisode = 12 };

        var result = NyaaCollectionReconciler.Reconcile(release, previous);

        var newRelease = Assert.IsType<ReconciliationResult.NewRelease.BatchRelease>(result);
        Assert.Equal(ReleaseContentType.BdRemux, newRelease.ContentType);
    }

    [Fact]
    public void Should_Not_Regress_Confirmation_When_BdRemux_Repeats_Already_Confirmed_Episodes()
    {
        var release = Release(21, new ReleaseContent.Batch(1, 12), isBdRemux: true);
        var previous = new NyaaConfirmation(21) { LastConfirmedEpisode = 12 };

        var result = NyaaCollectionReconciler.Reconcile(release, previous);

        var newRelease = Assert.IsType<ReconciliationResult.NewRelease.BatchRelease>(result);
        Assert.Equal(12, newRelease.UpdatedConfirmation.LastConfirmedEpisode);
    }

    [Fact]
    public void Should_Advance_Confirmation_When_BdRemux_Introduces_A_Higher_Episode()
    {
        var release = Release(21, new ReleaseContent.SingleEpisode(13), isBdRemux: true);
        var previous = new NyaaConfirmation(21) { LastConfirmedEpisode = 12 };

        var result = NyaaCollectionReconciler.Reconcile(release, previous);

        var newRelease = Assert.IsType<ReconciliationResult.NewRelease.SingleEpisode>(result);
        Assert.Equal(13, newRelease.UpdatedConfirmation.LastConfirmedEpisode);
    }

    #endregion

    #region Test Helpers

    private static MatchedRelease Release(int seriesId, ReleaseContent content, bool isBdRemux = false) =>
        new(seriesId,
            new NyaaEntry("Title", "https://nyaa.si/download/1.torrent", "https://nyaa.si/view/1", DateTimeOffset.UtcNow),
            content,
            isBdRemux);

    #endregion
}
