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
        private const string EraiRss = "https://spa.erai-raws.info/";
        private const string TitlePattern = @"(?<=\[720p\]\s)(.*?)(?=\s-\s\d+)";
        private const string EpisodeInfoPattern = @"(?<=\s-\s)\d+(\s\(V\d{1}\))?";
        public Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution)
        {
            try
            {
                var rssFeed = XDocument.Load(GetRssUrl(resolution));
                var feed = rssFeed.Descendants("item").Select(Mapper);
                return Right<DomainError, ImmutableList<FeedInfo>>(
                    feed.Where(f => f.PublicationDate >= DateTime.Today)
                        .ToImmutableList());
            }
            catch (Exception e)
            {
                return Left<DomainError, ImmutableList<FeedInfo>>(ExceptionError.FromException(e, "Erai_Feed_Exception"));
            }
        }

        private FeedInfo Mapper(XElement item)
        {
            var cleanedTitle = item.Element("title")?.Value.ReplaceKnownProblematicCharacters();
            return new FeedInfo(
                NonEmptyString.FromString(MatchPattern(TitlePattern, cleanedTitle ?? string.Empty)),
                NonEmptyString.FromString(item.Element("title")?.Value ?? string.Empty),
                DateTime.Parse(item.Element("pubDate")?.Value ?? string.Empty),
                item.Element("link")?.Value ?? string.Empty,
                MatchPattern(EpisodeInfoPattern, cleanedTitle ?? string.Empty));
        }

        private static string MatchPattern(string pattern, string str)
        {
            return Regex.Match(str, pattern, RegexOptions.IgnoreCase).Value;
        }

        private static string GetRssUrl(Resolution resolution)
        {
            return $"{EraiRss}/rss-{resolution}/";
        }
    }
}