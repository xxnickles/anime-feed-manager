using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.AniDb;
using AnimeFeedManager.Features.Scrapping.Types;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

internal static class TvSeriesScrapping
{
    internal static Task<Result<ScrapTvLibraryData>> ScrapSeries(
        this Task<Result<ImmutableList<FeedData>>> feedTitles,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions) =>
        feedTitles.Bind(titles => GetInitialProcessData(titles, season, puppeteerOptions));

    private static async Task<Result<ScrapTvLibraryData>> GetInitialProcessData(
        ImmutableList<FeedData> feedData,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions)
    {
        try
        {
            var (series, jsonSeason) =
                await AniDbScrapper.Scrap(Utils.CreateScrappingLink(season), puppeteerOptions);

            return (jsonSeason.Season, jsonSeason.Year, season is Latest)
                .ParseAsSeriesSeason()
                .WithOperationName(nameof(GetInitialProcessData))
                .AddLogOnSuccess((seriesSeason => logger =>
                    logger.LogInformation("{Count} series has been scrapped for season {Season}", series.Count(),
                        seriesSeason)))
                .WithLogProperty("Season", season)
                .Map(seriesSeason => new ScrapTvLibraryData(
                    series.Select(s => ToStorageData(s, jsonSeason)),
                    feedData,
                    seriesSeason));
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    // To keep it simple, we are using the season information coming from the scrapping instead of the parsed one
    // But at this point we are sure Season is valid as we have parsed the data scrapped
    private static StorageData ToStorageData(SeriesContainer seriesContainer, JsonSeasonInfo jsonSeason)
    {
        return new StorageData(
            new AnimeInfoStorage
            {
                RowKey = seriesContainer.Id,
                PartitionKey = IdHelpers.GenerateAnimePartitionKey(jsonSeason.Season, (ushort)jsonSeason.Year),
                Title = seriesContainer.Title,
                Synopsis = seriesContainer.Synopsys,
                FeedTitle = null,
                FeedLink = null,
                Date = SharedUtils.ParseDate(seriesContainer.Date, seriesContainer.SeasonInfo.Year)
                    ?.ToUniversalTime(),
                Status = SeriesStatus.NotAvailableValue
            },
            Uri.TryCreate(seriesContainer.ImageUrl, UriKind.Absolute, out var validUri)
                ? new ScrappedImageUrl(validUri)
                : new NoImage(),
            Status.NewSeries);
    }
}
