using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Movies.Library.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Feed;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Movies.Series;

public class OnScrapMoviesSeasonFeed
{
    private readonly IMoviesSeasonalLibrary _moviesProvider;
    private readonly MovieFeedUpdater _updater;
    private readonly ILogger<OnScrapMoviesSeasonFeed> _logger;

    public OnScrapMoviesSeasonFeed(
        IMoviesSeasonalLibrary moviesProvider,
        MovieFeedUpdater updater,
        ILogger<OnScrapMoviesSeasonFeed> logger)
    {
        _moviesProvider = moviesProvider;
        _updater = updater;
        _logger = logger;
    }

    [Function(nameof(OnScrapMoviesSeasonFeed))]
    public async Task Run(
        [QueueTrigger(ScrapMoviesSeasonFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        ScrapMoviesSeasonFeed message, CancellationToken token)
    {
        _logger.LogInformation("Processing movies feed for season {Year}-{Season}", message.SeasonInformation.Year,
            message.SeasonInformation.Season);

        var results = await SeasonValidators.Parse(message.SeasonInformation.Season, message.SeasonInformation.Year)
            .BindAsync(parsedSeason =>
                _moviesProvider.GetMoviesForFeedProcess(parsedSeason.Season, parsedSeason.Year, token))
            .BindAsync(movies => _updater.TryGetFeed(movies, token));

        results.Match(
            count => _logger.LogInformation("Trying to get feed information for {Count} movies from {Season}-{Year} ",
                count, message.SeasonInformation.Season, message.SeasonInformation.Year),
            error => error.LogError(_logger));
    }
}