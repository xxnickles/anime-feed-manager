using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.SeriesCompletion;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnTvSeriesComplete
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnTvSeriesComplete> _logger;

    public OnTvSeriesComplete(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnTvSeriesComplete> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnTvSeriesComplete))]
    public async Task Run([QueueTrigger(CompleteOngoingSeries.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)] CompleteOngoingSeries message,
    CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnTvSeriesComplete));
        await CompleteOngoing.CompleteOngoingSeries(message.Feed, _tableClientFactory, _domainPostman, token)
            .AddLogOnSuccess(r => logger => logger.LogInformation("TV Series have been completed {@Series}", r.CompletedSeries))
            .WriteLogs(_logger)
            .Done();
    }
}