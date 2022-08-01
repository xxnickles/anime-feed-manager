using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Services.Collectors.Interface;
using PuppeteerSharp;

namespace AnimeFeedManager.Services.Collectors.AniDb;

public class LibraryProvider : ILibraryProvider
{
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

    public LibraryProvider(PuppeteerOptions puppeteerOptions)
    {
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, (ImmutableList<AnimeInfo> Series, ImmutableList<ImageInformation> Images)>> GetLibrary(ImmutableList<string> feedTitles)
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                DefaultViewport = new ViewPortOptions {Height = 1080, Width = 1920},
                ExecutablePath = _puppeteerOptions.Path
            });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://anidb.net/anime/season/?type.tvseries=1");
            await page.WaitForSelectorAsync("div.g_bubblewrap.g_bubble.container");
            var data = await page.EvaluateFunctionAsync<JsonAnimeInfo[]>(ScrappingScript);
            await browser.CloseAsync();

            var package = data.Select(Map);

            return (
                package.Select(i => Map(i, feedTitles))
                    .ToImmutableList(),
                package.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                    .Select(Map)
                    .ToImmutableList()
            );
        }
        catch (Exception ex)
        {
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
        var sample = container.Date.Replace(" at", string.Empty).Replace("UTC", "GMT");
        var result = DateTime.TryParse(sample, out var date);
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
            new SeasonInfoDto(container.SeasonInfo.Season, container.SeasonInfo.Year));
    }
}