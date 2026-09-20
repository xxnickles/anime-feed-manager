using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public delegate Task<Result<ScrapTvLibraryData>> TvScrapper(SeasonSelector season, CancellationToken token);

public interface ITvLibraryScrapper
{
    Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default);
}

internal sealed class TvLibraryScrapper : ITvLibraryScrapper
{
    private readonly ISeasonFeedDataProvider _seasonFeedDataProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IAnimeScheduleClient _animeScheduleClient;

    public TvLibraryScrapper(
        ISeasonFeedDataProvider seasonFeedDataProvider,
        ITableClientFactory tableClientFactory,
        IAnimeScheduleClient animeScheduleClient)
    {
        _seasonFeedDataProvider = seasonFeedDataProvider;
        _tableClientFactory = tableClientFactory;
        _animeScheduleClient = animeScheduleClient;
    }

    public Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default)
    {
        return _seasonFeedDataProvider.Get()
            .ScrapSeries(_animeScheduleClient, season, token)
            .AddDataFromStorage(_tableClientFactory.TableStorageExistentStoredSeries, token);
    }
}