using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public interface ITvLibraryScrapper
{
    Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default);
}

internal sealed class TvLibraryScrapper : ITvLibraryScrapper
{
    private readonly ISeasonFeedDataProvider _seasonFeedDataProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly TimeProvider _timeProvider;

    public TvLibraryScrapper(
        ISeasonFeedDataProvider seasonFeedDataProvider,
        ITableClientFactory tableClientFactory,
        PuppeteerOptions puppeteerOptions,
        TimeProvider timeProvider)
    {
        _seasonFeedDataProvider = seasonFeedDataProvider;
        _tableClientFactory = tableClientFactory;
        _puppeteerOptions = puppeteerOptions;
        _timeProvider = timeProvider;
    }

    public Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default)
    {
        return _seasonFeedDataProvider.Get()
            .ScrapSeries(season, _puppeteerOptions)
            .AddDataFromStorage(
                _tableClientFactory.TableStorageExistentStoredSeries,
                _timeProvider,
                token);
    }
}