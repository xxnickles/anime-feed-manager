using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Tv.Scrapping.Series;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Tv.Series;

public class OnAlternativeTitleUpdate
{
    private readonly AlternativeTitleUpdater _alternativeTitleUpdater;
    private readonly ILogger<OnAlternativeTitleUpdate> _logger;

    public OnAlternativeTitleUpdate(
        AlternativeTitleUpdater alternativeTitleUpdater,
        ILogger<OnAlternativeTitleUpdate> logger)
    {
        _alternativeTitleUpdater = alternativeTitleUpdater;
        _logger = logger;
    }

    [Function(nameof(OnAlternativeTitleUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateAlternativeTitle.TargetQueue, Connection = Constants.AzureConnectionName)]
        UpdateAlternativeTitle message,
        CancellationToken token)
    {
        var result = await (PartitionKey.Validate(message.Season), RowKey.Validate(message.Id))
            .Apply((key, rowKey) => new {PartitionKey = key, RowKey = rowKey})
            .ValidationToEither()
            .BindAsync(safeData =>
                _alternativeTitleUpdater.AddAlternativeTitle(safeData.RowKey, safeData.PartitionKey, message.Original,
                    message.Title,
                    (SeriesStatus)message.Status,
                    token));

        result.Match(
            r => LogResult(r, message),
            error => error.LogError(_logger));
    }

    private void LogResult(AlternativeTitleUpdateResult result, UpdateAlternativeTitle message)
    {
        switch (result)
        {
            case AlternativeTitleUpdateResult.ProcessComplete:
                _logger.LogInformation(
                    "'{Title}' has been added as alternative title for {Id} and feed information has been updated for it",
                    message.Title,
                    message.Id);
                break;
            case AlternativeTitleUpdateResult.TitleAddedNotFeedFound:
                _logger.LogInformation(
                    "'{Title}' has been added as alternative title for {Id} but not feed information matched this alternative title",
                    message.Title,
                    message.Id);
                break;
        }
    }
}