using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Scrapping;

public sealed class OnTvTitlesScrapRequest(
    ScrapSeasonTitles titlesScrapper,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnTvTitlesScrapRequest> _logger = loggerFactory.CreateLogger<OnTvTitlesScrapRequest>();

    [Function(nameof(OnTvTitlesScrapRequest))]
    public async Task Run(
        [QueueTrigger(ScrapTvTilesRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        ScrapTvTilesRequest notification)
    {
        
        var result = await titlesScrapper.Scrap();
        result.Match(
            _ => _logger.LogInformation("Scrapping process for tv has started"),
            e => e.LogError(_logger)
        );
    }
}