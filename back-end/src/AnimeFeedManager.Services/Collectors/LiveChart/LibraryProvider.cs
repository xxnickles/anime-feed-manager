using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Services.Collectors.Interface;
using HtmlAgilityPack;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Services.Collectors.LiveChart
{
    public class LibraryProvider : IExternalLibraryProvider
    {
        private const string LiveChartLibrary = "https://www.livechart.me";

        private readonly IAsyncFeedTitlesProvider _titlesProvider;

        public LibraryProvider(IAsyncFeedTitlesProvider titlesProvider)
        {
            _titlesProvider = titlesProvider;
        }

        public async Task<Either<DomainError, ImmutableList<AnimeInfo>>> GetLibrary()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load(LiveChartLibrary);
                var seasonInfoString =
                    HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h1").InnerText);

                var (season, year) = GetSeasonInformation(seasonInfoString);
                var yearStr = OptionUtils.UnpackOption(year.Value, (ushort)DateTime.Today.Year).ToString();
                var feeTitles = await GetFeedTitles();

                var results = await Task.WhenAll(doc.DocumentNode
                    .SelectNodes("//main[@class='chart']/article[@class='anime']/div[@class='anime-card']")
                    .AsParallel()
                    .Where(FilterLeftover)
                    .Select(async x => await MapFromCard(x))
                    .Select(x => new AnimeInfo(
                        NonEmptyString.FromString(IdHelpers.GenerateAnimeId(season.Value, yearStr, x.Item1)),
                        NonEmptyString.FromString(x.Item1),
                        NonEmptyString.FromString(x.Item2),
                        NonEmptyString.FromString(Helpers.TryGetFeedTitle(feeTitles, x.Item1)),
                        new SeasonInformation(season, year),
                        x.Item3,
                        false)));


                return results.ToImmutableList();

            }
            catch (Exception e)
            {
                return ExceptionError.FromException(e, "Library");
            }
        }

        #region Extract Helpers

        private static (Season season, Year year) GetSeasonInformation(string nodeValue)
        {
            var parts = nodeValue.Split(' ');
            var season = Season.FromString(parts[0]);
            var result = int.TryParse(parts[1], out int year);

            if (!result) throw new ArgumentException("Year couldn't be extracted");

            return (season, new Year(year));
        }

        private static bool FilterLeftover(HtmlNode card)
        {
            // TODO: change for ongoing
            var extraInfo = card.SelectSingleNode("div[@class='anime-info']/div[@class='anime-date']");
            if (extraInfo is null) return true;

            return extraInfo.InnerText != "Ongoing";

        }

        private static async Task<(string title, string synopsys, Option<DateTime> date)> MapFromCard(HtmlNode card)
        {

            var title = WebUtility.HtmlDecode(card.SelectSingleNode("h3[@class='main-title']").InnerText);
            var animeInfo = card.SelectSingleNode("div[@class='anime-info']");
            
            var taskExtractSynopsis = ExtractSynopsis(animeInfo);
            var taskExtractDate = ExtractDate(animeInfo);
            var tasks = new Task[]
            {
                taskExtractSynopsis,
                taskExtractDate
            };

            await Task.WhenAll(tasks);
         
            var synopsis = await taskExtractSynopsis;
            var date = await taskExtractDate;

            return (title, synopsis, date);
        }

        private static Task<string> ExtractSynopsis(HtmlNode animeInfo)
        {
            return Task.Run(() =>
            {
                var synopsisParagraphs = animeInfo.SelectNodes("div[(@class='anime-synopsis') or (@class='anime-synopsis is-spoiler-masked')]/p");
                if (synopsisParagraphs is null || !synopsisParagraphs.Any()) return string.Empty;

                var paragraphsText = synopsisParagraphs.Select(x => HttpUtility.HtmlDecode(x.InnerText));
                return string.Join(Environment.NewLine, paragraphsText);
            });

        }

        private static Task<Option<DateTime>> ExtractDate(HtmlNode animeInfo)
        {
            return Task.Run(() =>
            {
                var dateString = animeInfo.SelectSingleNode("div[@class='anime-date']").InnerText;
                var cleanDateString = dateString.Replace("at", string.Empty).Replace("JST", "GMT");
                var result = DateTime.TryParse(cleanDateString, out DateTime date);
                return result ? Some(date) : None;
            });
        }

        private async Task<IEnumerable<string>> GetFeedTitles()
        {
            var titles = await _titlesProvider.GetTitles();
            return titles.Match(
                x => x,
                _ => ImmutableList<string>.Empty
            );
        }

        #endregion

    }
}
