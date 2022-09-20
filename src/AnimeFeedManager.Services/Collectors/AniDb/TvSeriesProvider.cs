using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Infrastructure;
using PuppeteerSharp;

namespace AnimeFeedManager.Services.Collectors.AniDb;

public class TvSeriesProvider : ITvSeriesProvider
{
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;

    private record JsonSeasonInfo(string Season, int Year);

    private record JsonAnimeInfo(string Title, string? ImageUrl, string Synopsys, string Date,
        JsonSeasonInfo SeasonInfo);

    private record SeriesContainer(string Id, string Title, string? ImageUrl, string Synopsys, string Date,
        JsonSeasonInfo SeasonInfo);

    private const string ScrappingScript = @"
        () => {
            const seasonInfomationGetter = () => {
                const getDate = (str) => str.includes('/') ? str.split('/')[1] : str;
                const formatSeason = (str) => str === 'autumn' ? 'fall' : str;
                const titleParts = document.querySelector('div.g_section.content > h2 span').innerText.split(' ');
                return {
                    season: formatSeason(titleParts[0].toLowerCase()),
                    year: parseInt(getDate(titleParts[1]))
                }
            }

            const seasonInfomation = seasonInfomationGetter();

            return [].slice.call(document.querySelectorAll('div.g_bubble.box'))
                .map(card => {

                    const getImage = () => {
                        const cleanSrc = (src) => src.replace('.jpg-thumb', '')
                        const image = card.querySelector('div.thumb.image img');
                        return cleanSrc(image.src);
                    }

                    const data = card.querySelector('div.data');
                    const title = data.querySelector('div.wrap.name a').innerText;
                    const synopsys = data.querySelector('div.desc')?.innerText ?? '';
                    const date = data.querySelector('div.date').innerText;

                    return {
                        title,
                        imageUrl: getImage(),
                        synopsys,
                        date,
                        seasonInfo: seasonInfomation
                    }
                }
                );
         }
    ";

    public TvSeriesProvider(
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions)
    {
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, TvSeries>> GetLibrary(ImmutableList<string> feedTitles)
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
                ExecutablePath = _puppeteerOptions.Path
            });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://anidb.net/anime/season/?type.tvseries=1");
            await page.WaitForSelectorAsync("div.g_bubblewrap.g_bubble.container");
            var data = await page.EvaluateFunctionAsync<JsonAnimeInfo[]>(ScrappingScript);
            await browser.CloseAsync();

            var package = data.Select(Map);

            var season = data.First().SeasonInfo;
            await _domainPostman.SendMessage(new SeasonProcessNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Information,
                new SeasonInfoDto(season.Season, season.Year),
                $"{package.Count()} series have been scrapped for {season.Season}-{season.Year}"));

            return new TvSeries(package.Select(i => Map(i, feedTitles))
                    .ToImmutableList(),
                package.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                    .Select(Map)
                    .ToImmutableList());
        }
        catch (Exception ex)
        {
            await _domainPostman.SendMessage(
                new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Error,
                    new NullSeasonInfo(),
                    "AniDb season scrapping failed"));
            return ExceptionError.FromException(ex, "LiveChartLibrary");
        }
    }

    private static SeriesContainer Map(JsonAnimeInfo info)
    {
        return new SeriesContainer(
            IdHelpers.GenerateAnimeId(info.SeasonInfo.Season, info.SeasonInfo.Year.ToString(), info.Title),
            info.Title,
            info.ImageUrl,
            info.Synopsys,
            info.Date,
            info.SeasonInfo);
    }

    private static AnimeInfo Map(SeriesContainer container, IEnumerable<string> feeTitles)
    {
        return new AnimeInfo(
            NonEmptyString.FromString(container.Id),
            NonEmptyString.FromString(container.Title),
            NonEmptyString.FromString(container.Synopsys),
            NonEmptyString.FromString(Helpers.TryGetFeedTitle(feeTitles, container.Title)),
            Map(container.SeasonInfo),
            ParseDate(container.Date, container.SeasonInfo.Year),
            false
        );
    }

    private static Option<DateTime> ParseDate(string dateStr, int year)
    {
        const string pattern = @"(\d{1,2})(\w{2})(\s\w+)";
        const string replacement = "$1$3";
        string dateCleaned = Regex.Replace(dateStr, pattern, replacement);
        var result = DateTime.TryParse($"{dateCleaned} {year}", out var date);
        return result ? Some(date) : None;
    }

    private static SeasonInformation Map(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }

    private static ImageInformation Map(SeriesContainer container)
    {
        return new ImageInformation(
            container.Id,
            IdHelpers.CleanAndFormatAnimeTitle(container.Title),
            container.ImageUrl,
            new SeasonInformation(Season.FromString(container.SeasonInfo.Season),
                Year.FromNumber(container.SeasonInfo.Year)));
    }
}