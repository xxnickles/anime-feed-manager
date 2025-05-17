using AnimeFeedManager.Features.Scrapping.AniDb;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class StartProcessStep
{
    internal static Task<Result<ScrapTvLibraryData>> StartProcessFromFeedTitles(
        this Task<Result<ImmutableList<string>>> titlesProvider,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions) =>
        titlesProvider
            .Bind(titles => GetProcessData(titles, season, puppeteerOptions));
  
    
    private static async Task<Result<ScrapTvLibraryData>> GetProcessData(
        ImmutableList<string> titles,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions,
        CancellationToken token = default)
    {
        // Scrapping
        var (series, jsonSeason) =
            await AniDbScrapper.Scrap(UpdateLibraryUtils.CreateScrappingLink(season), puppeteerOptions);

        return (jsonSeason.Season, jsonSeason.Year, season is Latest)
            .ParseAsSeriesSeason()
            .Map(seriesSeason => new ScrapTvLibraryData(
                IdHelpers.GetUniqueId(),
                series.Select(Transform),
                titles,
                seriesSeason));

        // To keep it simple, we are using the season information coming from the scrapping instead of the parsed one
        // But at this point we are sure Season is valid as we have parsed the data scrapped 
        StorageData Transform(SeriesContainer seriesContainer)
        {
            return new StorageData(new AnimeInfoStorage
                {
                    RowKey = seriesContainer.Id,
                    PartitionKey = IdHelpers.GenerateAnimePartitionKey(jsonSeason.Season, (ushort) jsonSeason.Year),
                    Title = seriesContainer.Title,
                    Synopsis = seriesContainer.Synopsys,
                    FeedTitle = string.Empty,
                    Date = Utils.ParseDate(seriesContainer.Date, seriesContainer.SeasonInfo.Year)?.ToUniversalTime(),
                    Status = SeriesStatus.NotAvailableValue,
                    Season = seriesContainer.SeasonInfo.Season,
                    Year = seriesContainer.SeasonInfo.Year
                }, 
                Uri.TryCreate(seriesContainer.ImageUrl, UriKind.Absolute, out var validUri)
                    ?  new ScrappedImageUrl(validUri): new NoImage());
        }
    }
   
}