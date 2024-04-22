using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Series;
using AnimeFeedManager.Features.Ovas.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Scrapping;

public sealed class OnTvTitlesScrapRequest(
    TvLibraryUpdater tvLibraryUpdater,
    OvasLibraryUpdater ovasLibraryUpdater,
    MoviesLibraryUpdater moviesLibraryUpdater,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnLibraryScrapRequest> _logger = loggerFactory.CreateLogger<OnLibraryScrapRequest>();

    [Function("OnTitlesScrapRequest")]
    public async Task Run(
        [QueueTrigger(Box.Available.LibraryScrapEventsBox, Connection = "AzureWebJobsStorage")]
        ScrapLibraryRequest notification)
    {
        var task = notification switch
        {
            (SeriesType.Tv, _, ScrapType.Latest) => tvLibraryUpdater.Update(new Latest()),
            (SeriesType.Tv, _, ScrapType.BySeason) => SeasonValidators.ParseSeasonValues(
                notification.SeasonParameter?.Season ?? string.Empty,
                notification.SeasonParameter?.Year ?? 0).BindAsync(season => tvLibraryUpdater.Update(season)),

            (SeriesType.Ova, _, ScrapType.Latest) => ovasLibraryUpdater.Update(new Latest()),
            (SeriesType.Ova, _, ScrapType.BySeason) => SeasonValidators.ParseSeasonValues(
                notification.SeasonParameter?.Season ?? string.Empty,
                notification.SeasonParameter?.Year ?? 0).BindAsync(season => ovasLibraryUpdater.Update(season)),

            (SeriesType.Movie, _, ScrapType.Latest) => moviesLibraryUpdater.Update(new Latest()),
            (SeriesType.Movie, _, ScrapType.BySeason) => SeasonValidators.ParseSeasonValues(
                notification.SeasonParameter?.Season ?? string.Empty,
                notification.SeasonParameter?.Year ?? 0).BindAsync(season => moviesLibraryUpdater.Update(season)),

            _ => Task.FromResult(
                Left<DomainError, Unit>(BasicError.Create($"Scrapping parameters are invalid {notification}"))
            )
        };

        var result = await task;
        result.Match(
            _ => _logger.LogInformation("Scrapping process for @{Parameters} has been completed successfully",
                notification),
            e => e.LogError(_logger)
        );
    }
}