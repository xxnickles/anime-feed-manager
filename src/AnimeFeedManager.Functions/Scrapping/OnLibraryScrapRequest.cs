using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Common.Domain.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping;
using AnimeFeedManager.Features.Ovas.Scrapping;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Scrapping;

public sealed class OnLibraryScrapRequest
{
    private readonly TvLibraryUpdater _tvLibraryUpdater;
    private readonly OvasLibraryUpdater _ovasLibraryUpdater;
    private readonly MoviesLibraryUpdater _moviesLibraryUpdater;
    private readonly ILogger<OnLibraryScrapRequest> _logger;

    public OnLibraryScrapRequest(
        TvLibraryUpdater tvLibraryUpdater,
        OvasLibraryUpdater ovasLibraryUpdater,
        MoviesLibraryUpdater moviesLibraryUpdater,
        ILoggerFactory loggerFactory)
    {
        _tvLibraryUpdater = tvLibraryUpdater;
        _ovasLibraryUpdater = ovasLibraryUpdater;
        _moviesLibraryUpdater = moviesLibraryUpdater;
        _logger = loggerFactory.CreateLogger<OnLibraryScrapRequest>();
    }

    [Function("OnLibraryScrapRequest")]
    public async Task Run(
        [QueueTrigger(Box.Available.LibraryScrapEventsBox, Connection = "AzureWebJobsStorage")]
        ScrapLibraryRequest notification)
    {
        var task = notification switch
        {
            (SeriesType.Tv, _, ScrapType.Latest) => _tvLibraryUpdater.Update(new Latest()),
            (SeriesType.Tv, _, ScrapType.BySeason) => _tvLibraryUpdater.Update(
                new BySeason(notification.SeasonParameter?.Season ?? string.Empty,
                    notification.SeasonParameter?.Year ?? 0)),

            (SeriesType.Ova, _, ScrapType.Latest) => _ovasLibraryUpdater.Update(new Latest()),
            (SeriesType.Ova, _, ScrapType.BySeason) => _ovasLibraryUpdater.Update(
                new BySeason(notification.SeasonParameter?.Season ?? string.Empty,
                    notification.SeasonParameter?.Year ?? 0)),

            (SeriesType.Movie, _, ScrapType.Latest) => _moviesLibraryUpdater.Update(new Latest()),
            (SeriesType.Movie, _, ScrapType.BySeason) => _moviesLibraryUpdater
                .Update(
                    new BySeason(notification.SeasonParameter?.Season ?? string.Empty,
                        notification.SeasonParameter?.Year ?? 0)),

            _ => Task.FromResult(
                Left<DomainError, Unit>(BasicError.Create($"Scrapping parameters are invalid {notification}"))
            )
        };

        var result = await task;
        result.Match(
            _ => _logger.LogInformation("Scrapping process for @{Parameters} has been completed successfully",
                notification),
            e => e.LogDomainError(_logger)
        );
    }
}