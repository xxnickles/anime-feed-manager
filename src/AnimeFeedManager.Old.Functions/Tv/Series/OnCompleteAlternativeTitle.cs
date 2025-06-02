using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Old.Features.Tv.Scrapping.Series.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Tv.Series;

public class OnCompleteAlternativeTitle
{
    private readonly AlternativeTitleUpdater _alternativeTitleUpdater;
    private readonly ILogger<OnCompleteAlternativeTitle> _logger;

    public OnCompleteAlternativeTitle(
        AlternativeTitleUpdater alternativeTitleUpdater,
        ILogger<OnCompleteAlternativeTitle> logger)
    {
        _alternativeTitleUpdater = alternativeTitleUpdater;
        _logger = logger;
    }

    [Function(nameof(OnCompleteAlternativeTitle))]
    public async Task Run(
        [QueueTrigger(CompleteAlternativeTitle.TargetQueue, Connection = Constants.AzureConnectionName)]
        CompleteAlternativeTitle message,
        CancellationToken token)
    {
        var result = await (PartitionKey.Validate(message.Partition), RowKey.Validate(message.Id))
            .Apply((key, rowKey) => new {PartitionKey = key, RowKey = rowKey})
            .ValidationToEither()
            .BindAsync(safeData =>
                _alternativeTitleUpdater.CompleteAlternativeTitle(safeData.RowKey, safeData.PartitionKey, token));

        result.Match(
            _ => _logger.LogInformation("Alternative title {Id} for season {Partition} has been completed successfully", message.Id, message.Partition),
            error => error.LogError(_logger));
    }

   
}