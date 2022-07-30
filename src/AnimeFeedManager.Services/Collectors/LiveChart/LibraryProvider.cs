using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Services.Collectors.Interface;
using PuppeteerSharp;

namespace AnimeFeedManager.Services.Collectors.LiveChart;
[Obsolete("Cloudflare locked")]
public class LibraryProvider : ILibraryProvider
{
    private readonly IFeedTitlesRepository _titlesRepository;

    private record JsonSeasonInfo(string Season, int Year);

    private record JsonAnimeInfo(string Title, string? ImageUrl, string Synopsys, string Date,
        JsonSeasonInfo SeasonInfo);

    private record JsonLibrary(JsonAnimeInfo[] AnimeInfo);

    private record SeriesContainer(string Id, string Title, string? ImageUrl, string Synopsys, string Date,
        JsonSeasonInfo SeasonInfo);


    private const string ScrappingScript = @"
        () => {
            // Set time zone to Universal Time
            const createCookie = (name, value) => {
                var cookie = [name, '=', encodeURIComponent(JSON.stringify(value)), '; domain=.', window.location.host.toString(), '; path=/;'].join('');
                document.cookie = cookie;
            }           
 
            createCookie('preferences', { 'time_zone': 'Etc/UTC' })

            const seasonInfomation = () => {
                const titleParts = document.querySelector('h1').innerText.split(' ');
                return {
                    season: titleParts[0].toLowerCase(),
                    year: parseInt(titleParts[1])
                }
            }

            const animeInfo = [].slice.call(document.querySelectorAll('div.anime-card'))
                .map(card => {
                    const checkIfLeftover = () => {
                        const extras = [].slice.call(card.querySelectorAll('div.anime-date'));
                        if (!extras) return false;
                        return extras.reduce((acc, curr) => acc || curr.innerText === 'Ongoing', false);
                    };

                    const getImage = () => {
                        const image = card.querySelector('div.poster-container > img');
                        const imgSrc = image.classList.contains('lazyload') ? image.getAttribute('data-src') : image.src;
                        return imgSrc.replace('small', 'large')
                    }

                    const animeInfo = card.querySelector('div.anime-info');
                    const synopsys = animeInfo.querySelector('div.anime-synopsis').innerText;
                    const date = animeInfo.querySelector('div.anime-date').innerText;

                    return {
                        title: card.querySelector('h3 > a').innerText,
                        image: getImage(),
                        leftover: checkIfLeftover(),
                        synopsys,
                        date

                    }
                }
                )
                .filter(info => !info.leftover)
                .map(x => ({
                    title: x.title,
                    imageUrl: x.image,
                    synopsys: x.synopsys,
                    date: x.date
                }));

            return {
                seasonInfo: seasonInfomation(),
                animeInfo
            }

        }
    ";


    public LibraryProvider(IFeedTitlesRepository titlesRepository)
    {
        _titlesRepository = titlesRepository;
    }

    public async Task<Either<DomainError, (ImmutableList<AnimeInfo> Series, ImmutableList<ImageInformation> Titles)>>
        GetLibrary()
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Args = new[] {"--no-sandbox", "--disable-setuid-sandbox"},
                Headless = false,
                DefaultViewport = new ViewPortOptions {Height = 1080, Width = 1920},
                Timeout = 0,
                Devtools = true
            });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://www.livechart.me");
            await page.WaitForSelectorAsync("header.site-header");
            var data = await page.EvaluateFunctionAsync<JsonLibrary>(ScrappingScript);
            var package = data.AnimeInfo.Select(Map);
            var feedTitles = await GetFeedTitles();

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

    private async Task<IEnumerable<string>> GetFeedTitles()
    {
        var titles = await _titlesRepository.GetTitles();
        return titles.Match(
            x => x,
            _ => ImmutableList<string>.Empty
        );
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
            result ? Some(date) : None,
            false
        );
    }

    private static ImageInformation Map(SeriesContainer container)
    {
        return new ImageInformation(container.Id, container.ImageUrl);
    }

    private static SeasonInformation Map(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }
}