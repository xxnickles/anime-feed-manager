using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Services.Collectors.HorribleSubs;
using FuzzySharp;
using HtmlAgilityPack;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Services.Collectors.LiveChart
{
    public class LibraryProvider : IExternalLibraryProvider
    {
        private const string LiveChartLibrary = "https://www.livechart.me";

        private readonly IFeedTitlesProvider _titlesProvider;

        public LibraryProvider(IFeedTitlesProvider titlesProvider)
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
                    HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h1[@class='chart-title']").InnerText);

                var (season, year) = GetSeasonInformation(seasonInfoString);
                var yearStr = OptionUtils.UnpackOption(year.Value, (ushort)DateTime.Today.Year).ToString();
                var feeTitles = GetFeedTitles();

                var results = await Task.WhenAll(doc.DocumentNode
                    .SelectNodes("//main[@class='chart']/article[@class='anime']/div[@class='anime-card']")
                    .AsParallel()
                    .Where(FilterLeftover)
                    .Select(async x => await MapFromCard(x))
                    .Select(x => new AnimeInfo(
                        NonEmptyString.FromString(GenerateId(season.Value, yearStr, x.Item1)),
                        NonEmptyString.FromString(x.Item2),
                        NonEmptyString.FromString(x.Item1),
                        NonEmptyString.FromString(x.Item3),
                        NonEmptyString.FromString(TryGetFeedTitle(feeTitles, x.Item1)),
                        season,
                        x.Item4,
                        year)));


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

            var extraInfo = card.SelectSingleNode("div[@class='poster-container']/div[@class='anime-extras']/div[@class='anime-extra']");
            if (extraInfo is null) return true;

            return extraInfo.InnerText != "Leftover";

        }

        private static async Task<(string title, string imgUrl, string synopsys, Option<DateTime> date)> MapFromCard(HtmlNode card)
        {

            var title = card.SelectSingleNode("h3[@class='main-title']").InnerText;
            var animeInfo = card.SelectSingleNode("div[@class='anime-info']");

            var taskExtractImage = ExtractImage(card);
            var taskExtractSynopsis = ExtractSynopsis(animeInfo);
            var taskExtractDate = ExtractDate(animeInfo);
            var tasks = new Task[]
            {
                taskExtractImage,
                taskExtractSynopsis,
                taskExtractDate
            };

            await Task.WhenAll(tasks);

            var imageUrl = await taskExtractImage;
            var synopsis = await taskExtractSynopsis;
            var date = await taskExtractDate;

            return (title, imageUrl, synopsis, date);
        }

        private static Task<string> ExtractImage(HtmlNode card)
        {
            return Task.Run(() =>
            {
                // Sometimes the scrapper takes a lazy loaded image
                var imageLazy = card.SelectSingleNode("div[@class='poster-container']/noscript/img");
                if (!(imageLazy is null)) return imageLazy.GetAttributeValue("src", string.Empty);

                // Sometimes doesn't
                var img = card.SelectSingleNode("div[@class='poster-container']/img");
                return img is null ? string.Empty : img.GetAttributeValue("src", string.Empty);
            });

        }

        private static Task<string> ExtractSynopsis(HtmlNode animeInfo)
        {
            return Task.Run(() =>
            {
                var synopsisParagraphs = animeInfo.SelectNodes("div[@class='anime-synopsis']/p");
                if (synopsisParagraphs is null || !synopsisParagraphs.Any()) return string.Empty;

                var paragraphsText = synopsisParagraphs.Select(x => x.InnerText);
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

        private IEnumerable<string> GetFeedTitles()
        {
            return _titlesProvider.GetTitles().Match(
                x => x,
                _ => ImmutableList<string>.Empty
            );
        }

        #endregion

        #region Other Helpers

        private static string GenerateId(string season, string year, string title)
        {
            var noSpecialCharactersString = Regex.Replace(title, "[^a-zA-Z0-9_.\\s]+", "", RegexOptions.Compiled);
            var cleanedString = noSpecialCharactersString
                .Replace(" ", "_")
                .Replace("__", "_");
            return $"{season}_{year}_{cleanedString}".ToLowerInvariant();
        }

        private static string TryGetFeedTitle(IEnumerable<string> titleList, string animeTitle)
        {
            var result = Process.ExtractOne(animeTitle, titleList);
            return result.Score switch
            {
                var s when s > 85 => result.Value,
                _ => string.Empty,
            };
        }

        #endregion
    }
}
