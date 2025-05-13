using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Messages;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tv.Library.UpdateProcess;

public sealed class UpdateLibrary
{
    
    private readonly ISeasonFeedTitlesProvider _seasonFeedTitlesProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<UpdateLibrary> _logger;

    public UpdateLibrary(
        ISeasonFeedTitlesProvider seasonFeedTitlesProvider,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        PuppeteerOptions puppeteerOptions,
        TimeProvider timeProvider,
        ILogger<UpdateLibrary> logger)
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
            // Get Season Feed Titles
            var t = _seasonFeedTitlesProvider.Get()
                .StartProcess(season, _puppeteerOptions)
                .AddStorageSeriesData(ExistentSeriesGetter.GetStoredSeries(_tableClientFactory), token)
                .AddLocalDataToScrappedSeries(_timeProvider);
                // .StartProcess(season, _puppeteerOptions)
                // .AddStorageSeriesData(_existentSeriesGetter, token)
                // .AddLocalDataToScrappedSeries(_timeProvider);
           

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