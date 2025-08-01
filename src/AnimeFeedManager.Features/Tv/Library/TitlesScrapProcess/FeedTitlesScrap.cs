using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;

namespace AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

public static class FeedTitlesScrap
{
    public static Task<Result<FeedTitleUpdateData>> StartFeedUpdateProcess(LatestSeasonGetter seasonGetter,
        CancellationToken token) =>
        seasonGetter(token).Bind(season =>
        {
            return season switch
            {
                LatestSeason latest => (latest.Season.Season ?? string.Empty, latest.Season.Year, latest.Season.Latest)
                    .ParseAsSeriesSeason()
                    .Map(latestSeason => new FeedTitleUpdateData(latestSeason, [], [])),
                NoMatch => new OperationError(
                    $"{nameof(StartFeedUpdateProcess)}",
                    "There is no latest season data in storage."),
                _ => new OperationError(
                    $"{nameof(StartFeedUpdateProcess)}-{nameof(seasonGetter)}",
                    $"Season is not latest. Received {season.GetType().Name}")
            };
        });


    public static Task<Result<FeedTitleUpdateData>> GetFeedTitles(this Task<Result<FeedTitleUpdateData>> data,
        ISeasonFeedTitlesProvider seasonFeedTitlesProvider) =>
        data.Bind(d => seasonFeedTitlesProvider.Get()
            .Map(titles => d with {FeedTitles = titles}));


    public static Task<Result<FeedTitleUpdateData>> UpdateSeries(
        this Task<Result<FeedTitleUpdateData>> data,
        RawStoredSeriesGetter seriesGetter, TvLibraryStorageUpdater seriesUpdater, CancellationToken token) =>
        data.Bind(d => UpdateSeries(d, seriesGetter, seriesUpdater, token));


    private static Task<Result<FeedTitleUpdateData>> UpdateSeries(
        FeedTitleUpdateData data,
        RawStoredSeriesGetter seriesGetter,
        TvLibraryStorageUpdater updater,
        CancellationToken token)
    {
        return seriesGetter(data.Season, token)
            .Map(series => series.ConvertAll(s => Transform(s, data.FeedTitles)))
            .Bind(seriesData => StoreChanges(seriesData, updater, token))
            .Map(seriesData => data with {FeedTitleUpdateInformation = seriesData});
    }

    private static FeedTitleUpdateInformation Transform(AnimeInfoStorage entity, ImmutableList<string> feedTitles)
    {
        var alternativeTitles = entity.AlternativeTitles?.Split(SharedUtils.ArraySeparator) ?? [];
        var matchTitle = feedTitles.TryGetFeedTitle(entity.Title ?? string.Empty);
         
        // If no match found with the main title, try with alternative titles
        if (string.IsNullOrWhiteSpace(matchTitle))
        {
            foreach (var altTitle in alternativeTitles)
            {
                matchTitle = feedTitles.TryGetFeedTitle(altTitle);
                if (!string.IsNullOrEmpty(matchTitle))
                    break; 
            }
        }
        
        if (string.IsNullOrWhiteSpace(matchTitle))
            return new FeedTitleUpdateInformation(entity, UpdateStatus.NoChanges);

        entity.FeedTitle = matchTitle;
        return new FeedTitleUpdateInformation(entity, UpdateStatus.Updated);
    }

    private static Task<Result<ImmutableList<FeedTitleUpdateInformation>>> StoreChanges(
        ImmutableList<FeedTitleUpdateInformation> data, TvLibraryStorageUpdater updater, CancellationToken token)
    {
        return updater(data.Where(d => d.UpdateStatus == UpdateStatus.Updated)
            .Select(d => d.Series), token).Map(_ => data);
    }
}