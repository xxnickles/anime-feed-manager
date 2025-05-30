using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public interface ITvLibraryScrapper
{
    Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default);
}

internal sealed class TvLibraryScrapper : ITvLibraryScrapper
{
    private readonly ISeasonFeedTitlesProvider _seasonFeedTitlesProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly TimeProvider _timeProvider;

    public TvLibraryScrapper(
        ISeasonFeedTitlesProvider seasonFeedTitlesProvider,
        ITableClientFactory tableClientFactory,
        PuppeteerOptions puppeteerOptions,
        TimeProvider timeProvider)
    {
        _seasonFeedTitlesProvider = seasonFeedTitlesProvider;
        _tableClientFactory = tableClientFactory;
        _puppeteerOptions = puppeteerOptions;
        _timeProvider = timeProvider;
    }

    public Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default)
    {
        return _seasonFeedTitlesProvider.Get()
            .ScrapSeries(season, _puppeteerOptions)
            .AddDataFromStorage(
                _tableClientFactory.GetExistentStoredSeriesGetter(),
                _timeProvider,
                token);
    }
}