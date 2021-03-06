﻿using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using LanguageExt;
using System;
using System.Collections.Generic;
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
                    ExceptionError.FromException(e, $"Erai_Feed_Exception"));
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
            var baseUrl = $"{EraiRss}/rss-{resolution}";

            return type switch
            {
                LinkType.Magnet => $"{baseUrl}-magnet",
                _ => baseUrl
            };
        }
    }
}