using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Messages;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public sealed class ScrapTvLibrary
{
    private readonly ISeasonFeedTitlesProvider _seasonFeedTitlesProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ScrapTvLibrary> _logger;

    public ScrapTvLibrary(
        ISeasonFeedTitlesProvider seasonFeedTitlesProvider,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions,
        TimeProvider timeProvider,
        ILogger<ScrapTvLibrary> logger)
    {
        _seasonFeedTitlesProvider = seasonFeedTitlesProvider;
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _puppeteerOptions = puppeteerOptions;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<Unit>> Execute(SeasonSelector season, CancellationToken token = default)
    {
        try
        {
            var t = _seasonFeedTitlesProvider.Get()
                .StartProcessFromFeedTitles(season, _puppeteerOptions)
                .AddLocalDataToScrappedSeries(
                    ExistentSeries.GetStoredSeries(_tableClientFactory),
                    _timeProvider,
                    token);
                
                // Uploading scrapped data to Azure Storage and adding it to the library
                //.UpdateTvLibrary(TvLibraryStore.UpsertLibrary(_tableClientFactory), token);
               
                // Rising events to notify the user that the library has been updated
                // Sending notifications to the users that are interested in the series
                // Rising Updated Season Event


            return Result<Unit>.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when updating TV library");
            return Result<Unit>.Failure(new HandledError());
        }
    }

    private Task<Result<Unit>> SendUpdateEvent(AnimeInfoStorage series, CancellationToken token = default)
    {
        return _domainPostman.SendMessage(new TvSeriesToUpdate(series), token);
    }
}