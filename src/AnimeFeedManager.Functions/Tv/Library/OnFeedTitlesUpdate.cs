using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.FeedTitlesUpdateProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnFeedTitlesUpdate
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnFeedTitlesUpdate> _logger;

    public OnFeedTitlesUpdate(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnFeedTitlesUpdate> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnFeedTitlesUpdate))]
    public async Task Run(
        [QueueTrigger(FeedTitlesUpdated.TargetQueue, Connection = Constants.AzureConnectionName)]
        FeedTitlesUpdated message, CancellationToken token)
    {
        await FeedTitlesUpdate.StoreTitles(new FeedTitleUpdateData(message.Season, message.FeedTitles),
                _tableClientFactory.GetFeedTitlesUpdater(), token)
            .SentEvents(_domainPostman, message.Season, token)
            .Match(
                _ => _logger.LogInformation("Feed titles have been updated"),
                e => e.LogError(_logger)
            );
    }
}