using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using LanguageExt;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Services.Collectors.Erai
{
    public class FeedProvider : IFeedProvider
    {
        private const string EraiRss = "https://www.erai-rss.info";
        private const string EraiPattern = @"(?<=\[720p\]\s)(.*?)(?=\s-\s\d+)";
        public Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution)
        {
            try
            {
                var rssFeed = XDocument.Load(GetRssUrl(resolution));
                var feed = from item in rssFeed.Descendants("item")
                           select new FeedInfo(
                               NonEmptyString.FromString(GetParsedTitle(item.Element("title")?.Value.ReplaceKnownProblematicCharacters() ?? string.Empty)),
                               NonEmptyString.FromString(item.Element("title")?.Value ?? string.Empty),
                               DateTime.Parse(item.Element("pubDate")?.Value ?? string.Empty),
                               item.Element("link")?.Value ?? string.Empty);

                return Right<DomainError, ImmutableList<FeedInfo>>(
                    feed.Where(f => f.PublicationDate >= DateTime.Today)
                        .ToImmutableList());
            }
            catch (Exception e)
            {
                return Left<DomainError, ImmutableList<FeedInfo>>(ExceptionError.FromException(e, "Erai_Feed_Exception"));
            }
        }

        private static string GetParsedTitle(string title)
        {
            return Regex.Match(title, EraiPattern, RegexOptions.IgnoreCase).Value;
        }

        private static string GetRssUrl(Resolution resolution)
        {
            return $"{EraiRss}/rss-{resolution.ToString()}/";
        }
    }
}