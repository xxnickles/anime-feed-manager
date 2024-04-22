using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Seasons.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public class OnStartScrapOvasFeed
{
    private readonly ISeasonsGetter _seasonsGetter;
    private readonly ILatestSeasonsGetter _latestSeasonsGetter;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnStartScrapOvasFeed> _logger;

    public OnStartScrapOvasFeed(
        ISeasonsGetter seasonsGetter,
        ILatestSeasonsGetter latestSeasonsGetter,
        IDomainPostman domainPostman,
        ILogger<OnStartScrapOvasFeed> logger)
    {
        _seasonsGetter = seasonsGetter;
        _latestSeasonsGetter = latestSeasonsGetter;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnStartScrapOvasFeed))]
    public async Task Run(
        [QueueTrigger(StartScrapOvasFeed.TargetQueue, Connection = Constants.AzureConnectionName)]
        StartScrapOvasFeed message, CancellationToken token)
    {
        var results = await GetProcessSeasons(message.Type, token)
            .BindAsync(seasons => SendMessages(seasons, token));
        
        results.Match(
            _ => _logger.LogInformation("Automated subscriptions will be processed for available users"),
            error => error.LogError(_logger));
    }

    private Task<Either<DomainError, IEnumerable<BasicSeason>>> GetProcessSeasons(FeedType type, CancellationToken token)
    {
        return type switch
        {
            FeedType.Complete => _seasonsGetter.GetAvailableSeasons(token)
                .MapAsync(seasons => seasons.Select(s => new BasicSeason(s.Season ?? string.Empty, (ushort) s.Year))),
            _ => _latestSeasonsGetter.Get(token)
                .MapAsync(seasons => seasons.Select(s => new BasicSeason(s.Season, s.Year))),
        };
    }

    private Task<Either<DomainError, Unit>> SendMessages(IEnumerable<BasicSeason> seasons, CancellationToken token)
    {
        return Task.WhenAll(seasons.Select(season => _domainPostman.SendMessage(new ScrapOvasSeasonFeed(season), token)))
            .FlattenResults()
            .MapAsync(_ => unit);

    }
}