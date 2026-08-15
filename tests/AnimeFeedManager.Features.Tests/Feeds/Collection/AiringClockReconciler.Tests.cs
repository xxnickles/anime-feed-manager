using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Types;

namespace AnimeFeedManager.Features.Tests.Feeds.Collection;

public class AiringClockReconcilerTests
{
    #region No previous flag

    [Fact]
    public void Should_Return_NoChange_When_No_Previous_Flag_And_Nothing_Has_Aired_Yet()
    {
        var clock = Clock(21, nextEpisode: 1);

        var result = AiringClockReconciler.Reconcile(clock, new AiringClockFlagLookup.NeverFlagged());

        Assert.IsType<AiringClockResult.NoChange>(result);
    }

    [Fact]
    public void Should_Return_Flagged_When_No_Previous_Flag_And_First_Episode_Has_Aired()
    {
        var clock = Clock(21, nextEpisode: 2);

        var result = AiringClockReconciler.Reconcile(clock, new AiringClockFlagLookup.NeverFlagged());

        var flagged = Assert.IsType<AiringClockResult.Flagged>(result);
        Assert.Equal(1, flagged.EpisodeStart);
        Assert.Equal(1, flagged.EpisodeEnd);
        Assert.Equal(21, flagged.UpdatedFlag.SeriesId);
        Assert.Equal(1, flagged.UpdatedFlag.LastFlaggedEpisode);
    }

    [Fact]
    public void Should_Return_Flagged_With_Full_Range_When_No_Previous_Flag_And_Several_Episodes_Have_Aired()
    {
        var clock = Clock(21, nextEpisode: 5);

        var result = AiringClockReconciler.Reconcile(clock, new AiringClockFlagLookup.NeverFlagged());

        var flagged = Assert.IsType<AiringClockResult.Flagged>(result);
        Assert.Equal(1, flagged.EpisodeStart);
        Assert.Equal(4, flagged.EpisodeEnd);
        Assert.Equal(4, flagged.UpdatedFlag.LastFlaggedEpisode);
    }

    #endregion

    #region Previous flag exists

    [Fact]
    public void Should_Return_NoChange_When_Next_Episode_Has_Not_Advanced_Past_Previous_Flag()
    {
        var clock = Clock(21, nextEpisode: 6);
        var previous = new AiringClockFlag(21) { LastFlaggedEpisode = 5 };

        var result = AiringClockReconciler.Reconcile(clock, new AiringClockFlagLookup.Found(previous));

        Assert.IsType<AiringClockResult.NoChange>(result);
    }

    [Fact]
    public void Should_Return_Flagged_When_A_Single_New_Episode_Has_Aired()
    {
        var clock = Clock(21, nextEpisode: 7);
        var previous = new AiringClockFlag(21) { LastFlaggedEpisode = 5 };

        var result = AiringClockReconciler.Reconcile(clock, new AiringClockFlagLookup.Found(previous));

        var flagged = Assert.IsType<AiringClockResult.Flagged>(result);
        Assert.Equal(6, flagged.EpisodeStart);
        Assert.Equal(6, flagged.EpisodeEnd);
        Assert.Equal(6, flagged.UpdatedFlag.LastFlaggedEpisode);
    }

    [Fact]
    public void Should_Return_Flagged_With_Gap_Range_When_Multiple_Episodes_Aired_Since_Last_Flag()
    {
        var clock = Clock(21, nextEpisode: 9);
        var previous = new AiringClockFlag(21) { LastFlaggedEpisode = 5 };

        var result = AiringClockReconciler.Reconcile(clock, new AiringClockFlagLookup.Found(previous));

        var flagged = Assert.IsType<AiringClockResult.Flagged>(result);
        Assert.Equal(6, flagged.EpisodeStart);
        Assert.Equal(8, flagged.EpisodeEnd);
        Assert.Equal(8, flagged.UpdatedFlag.LastFlaggedEpisode);
    }

    #endregion

    #region Test Helpers

    private static AniListEpisodeClock Clock(int malId, int nextEpisode) =>
        new(malId, nextEpisode, DateTimeOffset.UtcNow);

    #endregion
}
