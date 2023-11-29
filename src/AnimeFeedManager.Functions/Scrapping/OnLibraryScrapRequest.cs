using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Scrapping;

public sealed class OnLibraryScrapRequest(
    ScrapSeasonTitles titlesScrapper,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnLibraryScrapRequest> _logger = loggerFactory.CreateLogger<OnLibraryScrapRequest>();

    [Function("OnLibraryScrapRequest")]
    public async Task Run(
        [QueueTrigger(Box.Available.TvTitlesScrapEventsBox, Connection = "AzureWebJobsStorage")]
        ScrapTvTilesRequest notification)
    {
        
        var result = await titlesScrapper.Scrap();
        result.Match(
            _ => _logger.LogInformation("Scrapping process for tv has started"),
            e => e.LogDomainError(_logger)
        );
    }
}