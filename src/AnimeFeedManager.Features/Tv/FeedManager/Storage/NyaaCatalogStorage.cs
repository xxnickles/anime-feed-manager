using System.Globalization;
using AnimeFeedManager.Features.Scrapping.Nyaa;

namespace AnimeFeedManager.Features.Tv.FeedManager.Storage;

[WithTableName(AzureTableMap.StoreTo.NyaaCatalog)]
public sealed class NyaaCatalogStorage : ITableEntity
{
    public string? PartitionKey { get; set; } // series id
    public string? RowKey { get; set; } // {episode key}-{info hash}, see NyaaCatalogRows.RowKey
    public string Title { get; set; } = string.Empty;
    public string Magnet { get; set; } = string.Empty;
    public string Torrent { get; set; } = string.Empty;
    public string InfoHash { get; set; } = string.Empty;
    public DateTimeOffset Published { get; set; }
    public string Size { get; set; } = string.Empty;
    public bool Trusted { get; set; }
    public bool Revised { get; set; }
    public double EpisodeNumber { get; set; } // Tables has no decimal; specials like 12.5 are exact in a double
    public string EpisodeLabel { get; set; } = string.Empty;
    public bool Special { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public static class NyaaCatalogRows
{
    public static NyaaCatalogStorage ToCatalogRow(this NyaaRelease release, NyaaItem item, string seriesId) =>
        new()
        {
            PartitionKey = seriesId,
            RowKey = RowKey(release.Episode, item.InfoHash),
            Title = item.Title,
            Magnet = item.Magnet,
            Torrent = item.Torrent,
            InfoHash = item.InfoHash,
            Published = item.Published,
            Size = item.Size,
            Trusted = item.Trusted,
            Revised = release.Revised,
            EpisodeNumber = release.Episode switch
            {
                RegularEpisode regular => regular.Number,
                SpecialEpisode special => (double) special.Number,
                _ => throw new ArgumentOutOfRangeException(nameof(release), release.Episode, null)
            },
            EpisodeLabel = release.Episode.Label,
            Special = release.Episode is SpecialEpisode
        };

    /// <summary>Highest regular episode catalogued; 0 when there is none.</summary>
    public static int LatestRegular(this IEnumerable<NyaaCatalogStorage> catalog) =>
        catalog.Where(row => !row.Special).Select(row => (int) row.EpisodeNumber).DefaultIfEmpty(0).Max();

    /// <summary>Sorts by episode: <c>0012-</c> &lt; <c>0012.5-</c> &lt; <c>0013-</c>, since '-' precedes '.'.</summary>
    public static string RowKey(NyaaEpisode episode, string infoHash) =>
        $"{EpisodeKey(episode)}-{infoHash}";

    private static string EpisodeKey(NyaaEpisode episode) => episode switch
    {
        RegularEpisode regular => regular.Number.ToString("0000", CultureInfo.InvariantCulture),
        SpecialEpisode special => special.Number.ToString("0000.####", CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(episode), episode, null)
    };
}
