using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Interface;
using HtmlAgilityPack;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Services.Collectors.AniDb;

public class LibraryProvider : IExternalLibraryProvider
{
    internal class AnimeInfoContainer
    {
        internal string Title { get; }
        internal string Synopsis { get; }
        internal Option<DateTime> Date { get; }

        public AnimeInfoContainer(
            string title,
            string synopsis,
            Option<DateTime> date
        )
        {
            Title = title;
            Synopsis = synopsis;
            Date = date;
        }
    }

    private const string AniChartLibrary = "https://anidb.net/anime/season/?type.tvseries=1";

    private readonly IFeedTitlesRepository _titlesRepository;

    public LibraryProvider(IFeedTitlesRepository titlesRepository)
    {
        _titlesRepository = titlesRepository;
    }

    public async Task<Either<DomainError, ImmutableList<AnimeInfo>>> GetLibrary()
    {
        try
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(AniChartLibrary);
            var
                seasonInfoString =
                    HttpUtility.HtmlDecode(GetInnerTextOrEmpty(doc.DocumentNode, "//h1[@class='calendar']"));

            var (season, year, yearStr) = GetSeasonInformation(seasonInfoString);
            var feeTitles = await GetFeedTitles();
            return doc.DocumentNode
                .SelectNodes("//div[contains(@class,'g_bubblewrap') and contains(@class,'container')]/div[contains(@class,'g_bubble') and contains(@class,'box')]")
                .Select(n => MapFromCard(n, yearStr))
                .Select(aic => new AnimeInfo(
                    NonEmptyString.FromString(IdHelpers.GenerateAnimeId(season.Value, yearStr, aic.Title)),
                    NonEmptyString.FromString(aic.Title),
                    NonEmptyString.FromString(aic.Synopsis),
                    NonEmptyString.FromString(Helpers.TryGetFeedTitle(feeTitles, aic.Title)),
                    new SeasonInformation(season, year),
                    aic.Date,
                    false))
                .ToImmutableList();
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e, "AnichartLibrary");
        }
    }

    private static (Season season, Year year, string yearStr) GetSeasonInformation(string nodeValue)
    {
        static string GetDateString(string baseStr) =>
            baseStr.Contains('/') ? baseStr.Split('/')[1] : baseStr;

        var cleaned = nodeValue.Replace("Anime starting in ", string.Empty);
        var parts = cleaned.Split(' ');
        var season = Season.FromString(parts[0]);
        var yearStr = GetDateString(parts[1]);
        var result = int.TryParse(yearStr, out var year);

        if (!result) throw new ArgumentException("Year couldn't be extracted");

        return (season, new Year(year), yearStr);
    }

    private static AnimeInfoContainer MapFromCard(HtmlNode card, string yearStr)
    {
        var dataContainerNode = card.SelectSingleNode("div[@class='data']");
        var title = WebUtility.HtmlDecode(GetInnerTextOrEmpty(dataContainerNode, "div[@class='wrap name']/a"));
        var synopsis = WebUtility.HtmlDecode(GetInnerTextOrEmpty(dataContainerNode, "div[@class='desc']"));
        var date = ExtractDate(GetInnerTextOrEmpty(dataContainerNode, "div[@class='date']"), yearStr);

        return new AnimeInfoContainer(title, FormatSynopsis(synopsis), date);
    }

    private static Option<DateTime> ExtractDate(string dateStr, string yearStr)
    {
        const string pattern = @"(\d{1,2})(\w{2})(\s\w+)";
        const string replacement = "$1$3";
        string dateCleaned = Regex.Replace(dateStr, pattern, replacement);
        var result = DateTime.TryParse($"{dateCleaned} {yearStr}", out var date);
        return result ? Some(date) : None;
    }

    private async Task<IEnumerable<string>> GetFeedTitles()
    {
        var titles = await _titlesRepository.GetTitles();
        return titles.Match(
            x => x,
            _ => ImmutableList<string>.Empty
        );
    }

    private static string GetInnerTextOrEmpty(HtmlNode baseNode, string selector)
    {
        var node = baseNode.SelectSingleNode(selector);
        return node != null ? node.InnerText : string.Empty;
    }

    private static string FormatSynopsis(string original)
    {
        var cleaned = original.Replace("...", ".");
        return Regex.Replace(cleaned, @"(?<=[\w\d])[\.!?](?=[\w\d])", ". ");
    }
}