using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public class OnScrapOvasSeasonFeed
{
    private readonly IOvasSeasonalLibrary _ovasProvider;
    private readonly OvaFeedUpdater _updater;
    private readonly ILogger<OnScrapOvasSeasonFeed> _logger;

    public OnScrapOvasSeasonFeed(
        IOvasSeasonalLibrary ovasProvider,
        OvaFeedUpdater updater,
        ILogger<OnScrapOvasSeasonFeed> logger)
    {
        _ovasProvider = ovasProvider;
        _updater = updater;
        _logger = logger;
    }

    [Function(nameof(OnScrapOvasSeasonFeed))]
    public async Task Run(
        [QueueTrigger(ScrapOvasSeasonFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        ScrapOvasSeasonFeed message, CancellationToken token)
    {
        _logger.LogInformation("Processing ovas feed for season {Year}-{Season}", message.SeasonInformation.Year,
            message.SeasonInformation.Season);
        
        var results = await SeasonValidators.Parse(message.SeasonInformation.Season, message.SeasonInformation.Year)
            .BindAsync(parsedSeason =>
                _ovasProvider.GetOvasForFeedProcess(parsedSeason.Season, parsedSeason.Year, token))
            .BindAsync(ovas => _updater.TryGetFeed(ovas, token));

        results.Match(
            count => _logger.LogInformation("Trying to get feed information for {Count} Ovas from {Season}-{Year} ",
                count, message.SeasonInformation.Season, message.SeasonInformation.Year),
            error => error.LogError(_logger));
    }
}