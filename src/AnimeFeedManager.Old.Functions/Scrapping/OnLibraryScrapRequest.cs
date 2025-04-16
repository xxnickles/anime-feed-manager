using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Movies.Scrapping.Series;
using AnimeFeedManager.Features.Ovas.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Scrapping;

public sealed class OnLibraryScrapRequest(
    TvLibraryUpdater tvLibraryUpdater,
    OvasLibraryUpdater ovasLibraryUpdater,
    MoviesLibraryUpdater moviesLibraryUpdater,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnTvTitlesScrapRequest> _logger = loggerFactory.CreateLogger<OnTvTitlesScrapRequest>();

    [Function(nameof(OnLibraryScrapRequest))]
    public async Task Run(
        [QueueTrigger(ScrapLibraryRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        ScrapLibraryRequest notification, CancellationToken token)
    {
        var task = notification switch
        {
            (SeriesType.Tv, _, ScrapType.Latest, _) => tvLibraryUpdater.Update(new Latest(), token),
            (SeriesType.Tv, _, ScrapType.BySeason, _) => SeasonValidators.ParseSeasonValues(
                notification.SeasonParameter?.Season ?? string.Empty,
                notification.SeasonParameter?.Year ?? 0).BindAsync(season => tvLibraryUpdater.Update(season, token)),

            (SeriesType.Ova, _, ScrapType.Latest, _) => ovasLibraryUpdater.Update(new Latest(), notification.KeepFeed, token),
            (SeriesType.Ova, _, ScrapType.BySeason, _) => SeasonValidators.ParseSeasonValues(
                    notification.SeasonParameter?.Season ?? string.Empty,
                    notification.SeasonParameter?.Year ?? 0)
                .BindAsync(season => ovasLibraryUpdater.Update(season, notification.KeepFeed, token)),

            (SeriesType.Movie, _, ScrapType.Latest, _) => moviesLibraryUpdater.Update(new Latest(), notification.KeepFeed, token),
            (SeriesType.Movie, _, ScrapType.BySeason, _) => SeasonValidators.ParseSeasonValues(
                    notification.SeasonParameter?.Season ?? string.Empty,
                    notification.SeasonParameter?.Year ?? 0)
                .BindAsync(season => moviesLibraryUpdater.Update(season, notification.KeepFeed, token)),

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