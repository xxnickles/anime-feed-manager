using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public class OnScrapOvasSeasonFeed
{
    private readonly IOvasSeasonalLibrary _ovasProvider;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnScrapOvasSeasonFeed> _logger;

    public OnScrapOvasSeasonFeed(
        IOvasSeasonalLibrary ovasProvider,
        IDomainPostman domainPostman,
        ILogger<OnScrapOvasSeasonFeed> logger)
    {
        _ovasProvider = ovasProvider;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnScrapOvasSeasonFeed))]
    public async Task Run(
        [QueueTrigger(ScrapOvasSeasonFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        ScrapOvasSeasonFeed message, CancellationToken token)
    {
        var results = await SeasonValidators.Parse(message.SeasonInformation.Season, message.SeasonInformation.Year)
            .BindAsync(parsedSeason =>
                _ovasProvider.GetOvasForFeedProcess(parsedSeason.Season, parsedSeason.Year, token))
            .BindAsync(ovas => SendMessages(ovas, token));

        results.Match(
            count => _logger.LogInformation("Trying to get feed information for {Count} Ovas from {Season}-{Year} ",
                count, message.SeasonInformation.Season, message.SeasonInformation.Year),
            error => error.LogError(_logger));
    }

    private Task<Either<DomainError, int>> SendMessages(IEnumerable<OvaStorage> ovas, CancellationToken token)
    {
        return Task.WhenAll(
                ovas.Select(season => _domainPostman.SendMessage(new ScrapOvaFeed(season), token)))
            .FlattenResults()
            .MapAsync(r => r.Count);
    }
}