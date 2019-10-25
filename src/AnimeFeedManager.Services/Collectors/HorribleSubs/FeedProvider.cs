using AnimeFeedManager.Core.Error;
using LanguageExt;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Services.Collectors.HorribleSubs
{
    public class FeedProvider : IFeedProvider
    {
        private const string HorribleSubsRss = "http://horriblesubs.info/rss.php";
        private const string HorribleSubsPattern = @"(?<=\[HorribleSubs\]\s)(.*?)(?=\s-\s\d+\s\[\d+p\].mkv)";

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
                return Left<DomainError, ImmutableList<FeedInfo>>(ExceptionError.FromException(e, "HorribleSubs_Feed_Exception"));
            }
        }

        private static string GetParsedTitle(string title)
        {
            return Regex.Match(title, HorribleSubsPattern).Value;
        }

        private static string GetRssUrl(Resolution resolution)
        {
            return $"{HorribleSubsRss}?res={resolution.ToString()}";
        }
    }
}
