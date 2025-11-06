using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AnimeFeedManager.Features.Tv.Feed;

public delegate Result<ImmutableList<FeedInfo>> FeedProvider();

public static class SubPleaseFeedProvider
{
    private record struct FeedItem(
        string AnimeTitle,
        string FeedTitle,
        string EpisodeInfo,
        DateTime PublicationDate,
        string Link,
        LinkType Type);

    private const string SubsPleaseRss = "https://subsplease.org/rss/";
    private const string TitlePattern = @"(?<=\[SubsPlease\]\s)(.*?)(?=\s-\s\d+)";
    private const string EpisodeInfoPattern = @"(?<=\s-\s)\d+(\s\(V\d{1}\))?";
    private const string Resolution = "1080";

    public static Result<ImmutableList<FeedInfo>> GetFeed()
    {
        try
        {
            return GroupAndTransformFeedItems(
                new List<LinkType> {LinkType.TorrentFile, LinkType.Magnet}
                
                    .SelectMany(GetFeedInformation)
                .Where(f => f.PublicationDate >= DateTime.Today)).ToImmutableList();
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e)
                .WithOperationName(nameof(GetFeed));
        }
    }

    private static IEnumerable<FeedInfo> GroupAndTransformFeedItems(IEnumerable<FeedItem> feedItems)
    {
        return
            from source in feedItems.GroupBy(i => i.AnimeTitle)
            let links = source.Select(x => new TorrentLink(x.Type, x.Link)).ToImmutableList()
            let baseElement = source.First()
            select new FeedInfo(
                baseElement.AnimeTitle,
                baseElement.FeedTitle,
                baseElement.PublicationDate,
                links,
                baseElement.EpisodeInfo);
    }

    private static IEnumerable<FeedItem> GetFeedInformation(LinkType type)
    {
        var rssFeed = XDocument.Load(GetRssUrl(type));
        return rssFeed.Descendants("item").Select(item => AccumulatorMapper(item, type));
    }

    private static FeedItem AccumulatorMapper(XElement item, LinkType type)
    {
        var cleanedTitle = ReplaceKnownProblematicCharacters(item.Element("title")?.Value ?? string.Empty);
        return new FeedItem(
            MatchPattern(TitlePattern, cleanedTitle),
            item.Element("title")?.Value ?? string.Empty,
            MatchPattern(EpisodeInfoPattern, cleanedTitle),
            DateTime.Parse(item.Element("pubDate")?.Value ?? string.Empty),
            item.Element("link")?.Value ?? string.Empty,
            type);
    }

    private static string MatchPattern(string pattern, string str)
    {
        return Regex.Match(str, pattern, RegexOptions.IgnoreCase).Value;
    }

    private static string GetRssUrl(LinkType type)
    {
        const string baseUrl = $"{SubsPleaseRss}";

        return type switch
        {
            LinkType.Magnet => $"{baseUrl}?r={Resolution}",
            _ => $"{baseUrl}?t&r={Resolution}"
        };
    }

    private static string ReplaceKnownProblematicCharacters(string title)
    {
        return title.Replace('–', '-').Replace('-', '-');
    }
}