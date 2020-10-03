using AnimeFeedManager.Core.Error;
using HtmlAgilityPack;
using LanguageExt;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using AnimeFeedManager.Services.Collectors.Interface;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Services.Collectors.HorribleSubs
{
    [Obsolete("Horrible subs has been closed", false)]
    public class FeedTitles : IFeedTitlesProvider
    {
        private const string HorribleSubsFeed = "https://horriblesubs.info/release-schedule";

        public Either<DomainError, ImmutableList<string>> GetTitles()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load(HorribleSubsFeed);
                var baseNode = doc.DocumentNode.SelectSingleNode("//div[@class='entry-content']");
                if(baseNode == null) throw new ArgumentException("There is no content in the given url");
                var titleListStr = baseNode
                    .SelectNodes("//tr[@class='schedule-page-item']/td[@class='schedule-page-show']")
                    .Select(x => HttpUtility.HtmlDecode(x.InnerText).ReplaceKnownProblematicCharacters())
                    .ToImmutableList();

                return Right<DomainError, ImmutableList<string>>(titleListStr);

            }
            catch (Exception e)
            {
                return Left<DomainError, ImmutableList<string>>(ExceptionError.FromException(e, "HorribleSubs_Scraping_Exception"));
            }
        }
    }
}
