using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AnimeFeedManager.Services.Collectors.Interface;

namespace AnimeFeedManager.Services.Collectors.SubsPlease;

public class FeedProvider : IFeedProvider
{
    private const string SubsPleaseRss = "https://subsplease.org/rss/";
    private const string TitlePattern = @"(?<=\[SubsPlease\]\s)(.*?)(?=\s-\s\d+)";
    private const string EpisodeInfoPattern = @"(?<=\s-\s)\d+(\s\(V\d{1}\))?";

    public Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution)
    {
        try
        {
            var sources = new List<LinkType> { LinkType.TorrentFile, LinkType.Magnet }
                .SelectMany(type => GetFeedInformation(Resolution.Hd, type))
                .Where(f => f.PublicationDate >= DateTime.Today)
                .GroupBy(i => i.AnimeTitle);

            var resultList =
                from source in sources
                let links = source.Select(x => new TorrentLink(x.Type, x.Link)).ToImmutableList()
                let baseElement = source.First()
                select new FeedInfo(
                    NonEmptyString.FromString(baseElement.AnimeTitle),
                    NonEmptyString.FromString(baseElement.FeedTitle),
                    baseElement.PublicationDate,
                    links,
                    baseElement.EpisodeInfo);
            return resultList.ToImmutableList();
        }
        catch (Exception e)
        {
            return Left<DomainError, ImmutableList<FeedInfo>>(
                ExceptionError.FromException(e, "SubsPlease_Feed_Exception"));
        }
    }

    private IEnumerable<Accumulator> GetFeedInformation(Resolution resolution, LinkType type)
    {
        var rssFeed = XDocument.Load(GetRssUrl(resolution, type));
        return rssFeed.Descendants("item").Select(item => AccumulatorMapper(item, type));
    }

    private Accumulator AccumulatorMapper(XElement item, LinkType type)
    {
        var cleanedTitle = item.Element("title")?.Value.ReplaceKnownProblematicCharacters();
        return new Accumulator(
            MatchPattern(TitlePattern, cleanedTitle ?? string.Empty),
            item.Element("title")?.Value ?? string.Empty,
            MatchPattern(EpisodeInfoPattern, cleanedTitle ?? string.Empty),
            DateTime.Parse(item.Element("pubDate")?.Value ?? string.Empty),
            item.Element("link")?.Value ?? string.Empty,
            type);
    }

    private static string MatchPattern(string pattern, string str)
    {
        return Regex.Match(str, pattern, RegexOptions.IgnoreCase).Value;
    }

    private static string GetRssUrl(Resolution resolution, LinkType type)
    {
        var baseUrl = $"{SubsPleaseRss}";

        return type switch
        {
            LinkType.Magnet => $"{baseUrl}?r={resolution}",
            _ => $"{baseUrl}?t&r={resolution}"
        };
    }
}