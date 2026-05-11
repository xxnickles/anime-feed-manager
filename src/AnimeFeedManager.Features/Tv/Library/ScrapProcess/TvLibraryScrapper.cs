using AnimeFeedManager.Features.Scrapping.Jikan;
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
    private readonly IJikanClient _jikanClient;
    private readonly TimeProvider _timeProvider;

    public TvLibraryScrapper(
        ISeasonFeedDataProvider seasonFeedDataProvider,
        ITableClientFactory tableClientFactory,
        IJikanClient jikanClient,
        TimeProvider timeProvider)
    {
        _seasonFeedDataProvider = seasonFeedDataProvider;
        _tableClientFactory = tableClientFactory;
        _jikanClient = jikanClient;
        _timeProvider = timeProvider;
    }

    public Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default)
    {
        return _seasonFeedDataProvider.Get()
            .ScrapSeries(_jikanClient, season, token)
            .AddDataFromStorage(
                _tableClientFactory.TableStorageExistentStoredSeries,
                _timeProvider,
                token);
    }
}