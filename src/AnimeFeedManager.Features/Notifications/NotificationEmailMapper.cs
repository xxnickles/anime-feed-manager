using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;

namespace AnimeFeedManager.Features.Notifications;

/// <summary>
/// Flattens a <see cref="UserDigest"/> into one <see cref="ReleaseCardModel"/> per release — a
/// series with two new releases in one pass gets two cards, not one merged card.
/// </summary>
public static class NotificationEmailMapper
{
    public static NotificationDigestView Map(
        UserDigest digest,
        string recipientName,
        DateTimeOffset digestDate,
        IReadOnlyDictionary<int, string> seriesTitles,
        NyaaOptions nyaaOptions)
    {
        var cards = digest.Series
            .SelectMany(match => match.Releases.Select(release =>
                MapCard(match.Subscriber.SeriesId, release, seriesTitles, nyaaOptions)))
            .ToImmutableArray();

        return new NotificationDigestView(recipientName, digestDate, cards);
    }

    private static ReleaseCardModel MapCard(
        int seriesId,
        ReleaseDetected release,
        IReadOnlyDictionary<int, string> seriesTitles,
        NyaaOptions nyaaOptions)
    {
        var title = seriesTitles.GetValueOrDefault(seriesId, $"Series {seriesId}");

        // Nyaa pill only for Confirmed releases — an Untrackable series structurally never
        // appears on Nyaa, so the link would always be a dead search.
        var platforms = release.Confirmed
            ? release.Platforms.Select(p => ToPill(p.Name, p.Url))
                .Append(ToPill("Nyaa", NyaaSearchLink.Build(nyaaOptions, title, release.Episode)))
            : release.Platforms.Select(p => ToPill(p.Name, p.Url));

        return new ReleaseCardModel(
            title, release.ContentType, release.Episode, release.EpisodeRangeEnd, release.Confirmed,
            [.. platforms]);
    }

    private static PlatformPillModel ToPill(string name, string url)
    {
        var color = PlatformColors.For(name);
        return new PlatformPillModel(name, url, color.Background, color.Text);
    }
}
