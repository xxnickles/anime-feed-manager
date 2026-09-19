using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Web.BlazorComponents.Tests.Email.Templates;

public class AnimeFeedNotificationMapperTests
{
    #region Episode Ordering

    [Fact]
    public void Should_Order_Episodes_Newest_First_When_Feed_Is_Unordered()
    {
        var model = Notification(Feed("Some Series", "99", "101", "100")).ToEmailModel();

        Assert.Equal(["101", "100", "99"], EpisodeNumbers(model.AnimeFeeds[0]));
    }

    [Fact]
    public void Should_Order_Each_Series_Independently_When_Feed_Has_Several()
    {
        var model = Notification(
                Feed("First Series", "08", "10"),
                Feed("Second Series", "02", "02v2"))
            .ToEmailModel();

        Assert.Equal(["10", "08"], EpisodeNumbers(model.AnimeFeeds[0]));
        Assert.Equal(["02v2", "02"], EpisodeNumbers(model.AnimeFeeds[1]));
    }

    [Fact]
    public void Should_Return_Empty_Episodes_When_Feed_Has_None()
    {
        var model = Notification(Feed("Some Series")).ToEmailModel();

        Assert.Empty(model.AnimeFeeds[0].Episodes);
    }

    #endregion

    #region Episode Mapping

    [Fact]
    public void Should_Carry_Episode_Details_Onto_The_Reordered_Episode()
    {
        var model = Notification(Feed("Some Series", "09", "10")).ToEmailModel();

        var newest = model.AnimeFeeds[0].Episodes[0];

        Assert.Equal("10", newest.EpisodeNumber);
        Assert.Equal("magnet:?xt=urn:btih:10", newest.MagnetLink);
        Assert.Equal("https://example.com/torrent/10", newest.TorrentLink);
        Assert.True(newest.IsNew);
    }

    #endregion

    #region Test Helpers

    private static string[] EpisodeNumbers(AnimeFeedGroup series) =>
        series.Episodes.Select(episode => episode.EpisodeNumber).ToArray();

    private static FeedNotification Notification(params DailySeriesFeed[] feeds) =>
        new(new UserActiveSubscriptions("user1", "tester@example.com", []), feeds);

    private static DailySeriesFeed Feed(string title, params string[] episodeNumbers) =>
        new(title,
            "https://example.com/series",
            episodeNumbers.Select(Episode).ToArray());

    private static EpisodeData Episode(string episodeNumber) =>
        new(episodeNumber,
            $"magnet:?xt=urn:btih:{episodeNumber}",
            $"https://example.com/torrent/{episodeNumber}",
            true);

    #endregion
}
