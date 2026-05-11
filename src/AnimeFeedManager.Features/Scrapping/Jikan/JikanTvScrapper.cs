using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Scrapping.Jikan;

internal static class JikanTvScrapper
{
    internal static Task<Result<ScrapTvLibraryData>> ScrapSeries(
        this Task<Result<ImmutableArray<FeedData>>> feedTitles,
        IJikanClient jikanClient,
        SeasonSelector season,
        CancellationToken token) =>
        feedTitles.Bind(titles => GetInitialProcessData(titles, jikanClient, season, token));

    private static async Task<Result<ScrapTvLibraryData>> GetInitialProcessData(
        ImmutableArray<FeedData> feedData,
        IJikanClient jikanClient,
        SeasonSelector season,
        CancellationToken token)
    {
        var jikanResult = season switch
        {
            Latest => await jikanClient.GetCurrentSeason(token),
            BySeason s => await jikanClient.GetSeason(s.Year, s.Season, token),
            _ => throw new UnreachableException()
        };

        return jikanResult
            .Map(anime => anime.Where(a => a.Type == "TV").ToImmutableArray())
            .Bind(tvOnly => BuildResult(tvOnly, season, feedData));
    }

    private static Result<ScrapTvLibraryData> BuildResult(
        ImmutableArray<JikanAnime> tvSeries,
        SeasonSelector selector,
        ImmutableArray<FeedData> feedData)
    {
        var (seasonString, year) = selector switch
        {
            BySeason s => ((string)s.Season, (int)s.Year),
            Latest when tvSeries.Length > 0 =>
                (tvSeries[0].Season ?? string.Empty, tvSeries[0].Year ?? 0),
            _ => (string.Empty, 0)
        };

        return (seasonString, year, selector is Latest)
            .ParseAsSeriesSeason()
            .WithOperationName(nameof(GetInitialProcessData))
            .AddLogOnSuccess(seriesSeason => logger =>
                logger.LogInformation("{Count} TV series scraped from Jikan for {Season}",
                    tvSeries.Length, seriesSeason))
            .WithLogProperty("Season", selector)
            .Map(seriesSeason => new ScrapTvLibraryData(
                tvSeries.Select(a => ToStorageData(a, seriesSeason.Season, seriesSeason.Year)),
                feedData,
                seriesSeason));
    }

    private static StorageData ToStorageData(JikanAnime anime, Season season, Year year)
    {
        ImageInformation image =
            !string.IsNullOrWhiteSpace(anime.Images.Jpg.LargeImageUrl)
            && Uri.TryCreate(anime.Images.Jpg.LargeImageUrl, UriKind.Absolute, out var validUri)
                ? new ScrappedImageUrl(validUri)
                : new NoImage();

        return new StorageData(
            new AnimeInfoStorage
            {
                RowKey = IdHelpers.GenerateAnimeId(season, year.ToString(), anime.Title),
                PartitionKey = IdHelpers.GenerateAnimePartitionKey(season, year),
                Title = anime.Title,
                Synopsis = anime.Synopsis ?? string.Empty,
                FeedTitle = null,
                FeedLink = null,
                Date = anime.Aired?.From?.ToUniversalTime(),
                Status = SeriesStatus.NotAvailableValue
            },
            image,
            Status.NewSeries);
    }
}
