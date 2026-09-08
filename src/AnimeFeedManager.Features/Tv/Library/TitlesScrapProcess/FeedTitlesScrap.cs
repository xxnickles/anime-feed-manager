using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;

namespace AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

public static class FeedTitlesScrap
{
    public static Task<Result<FeedTitleUpdateData>> StartFeedUpdateProcess(LatestSeasonGetter seasonGetter,
        CancellationToken token) =>
        seasonGetter(token)
            .WithOperationName(nameof(StartFeedUpdateProcess))
            .Bind(season =>
            {
                return season switch
                {
                    CurrentLatestSeason latest => FeedUpdateDataFor(latest.Season),
                    FallbackLatestSeason newest => FeedUpdateDataFor(newest.Season),
                    NoMatch => Error.Create("There is no latest season data in storage."),
                    _ => Error.Create($"Season is not latest. Received {season.GetType().Name}")
                };
            });


    private static Result<FeedTitleUpdateData> FeedUpdateDataFor(SeasonStorage season) =>
        (season.Season ?? string.Empty, season.Year, season.Latest)
        .ParseAsSeriesSeason()
        .Map(parsed => new FeedTitleUpdateData(parsed, [], []));

    public static Task<Result<FeedTitleUpdateData>> GetFeedTitles(this Task<Result<FeedTitleUpdateData>> data,
        ISeasonFeedDataProvider seasonFeedDataProvider) =>
        data.Bind(d => seasonFeedDataProvider.Get()
            .Map(titles => d with {FeedData = titles}));


    public static Task<Result<FeedTitleUpdateData>> UpdateSeries(
        this Task<Result<FeedTitleUpdateData>> data,
        RawStoredSeries seriesGetter, TvLibraryStorageUpdater seriesUpdater, CancellationToken token) =>
        data.Bind(d => UpdateSeries(d, seriesGetter, seriesUpdater, token));


    private static Task<Result<FeedTitleUpdateData>> UpdateSeries(
        FeedTitleUpdateData data,
        RawStoredSeries seriesGetter,
        TvLibraryStorageUpdater updater,
        CancellationToken token)
    {
        return seriesGetter(data.Season, token)
            .Map(series => series.Select(s => Transform(s, data.FeedData))
                .Where(s => s.UpdateStatus is UpdateStatus.Updated)
                .ToImmutableArray()) // Remove series that have no changes
            .Bind(seriesData => StoreChanges(seriesData, updater, token))
            .Map(seriesData => data with {FeedTitleUpdateInformation = seriesData});
    }

    private static FeedTitleUpdateInformation Transform(AnimeInfoStorage entity, ImmutableArray<FeedData> feedTitles)
    {
        // Pass on series that already are ongoing
        if (entity.Status == SeriesStatus.Ongoing())
            return new FeedTitleUpdateInformation(entity, UpdateStatus.NoChanges);

        var alternativeTitles = StoredAlternativeTitles.Parse(entity.AlternativeTitles).ForMatching;
        var feedMatch = feedTitles.TryGetFeedMatch(entity.Title ?? string.Empty);

        // If no match found with the main title, try with alternative titles
        if (feedMatch is null)
        {
            foreach (var altTitle in alternativeTitles)
            {
                feedMatch = feedTitles.TryGetFeedMatch(altTitle);
                if (feedMatch is not null)
                    break;
            }
        }

        if (feedMatch is null)
            return new FeedTitleUpdateInformation(entity, UpdateStatus.NoChanges);

        entity.FeedTitle = feedMatch.Title;
        entity.FeedLink = feedMatch.Url;
        entity.Status = SeriesStatus.Ongoing();
        return new FeedTitleUpdateInformation(entity, UpdateStatus.Updated);
    }

    private static Task<Result<ImmutableArray<FeedTitleUpdateInformation>>> StoreChanges(
        ImmutableArray<FeedTitleUpdateInformation> data, TvLibraryStorageUpdater updater, CancellationToken token)
    {
        return updater(data.Where(d => d.UpdateStatus == UpdateStatus.Updated)
            .Select(d => d.Series), token).Map(_ => data);
    }
}