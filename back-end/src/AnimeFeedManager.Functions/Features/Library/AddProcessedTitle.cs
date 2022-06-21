using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class AddProcessedTitle
{
    [FunctionName("AddProcessedTitle")]
    [StorageAccount("AzureWebJobsStorage")]
    [return: Table(Tables.ProcessedTitles)]
    public ProcessedTitlesStorage Run(
        [QueueTrigger(QueueNames.ProcessedTitles)]
        string title,
        ILogger log)
    {
        log.LogInformation($"Saving {title}");
        var storeTitle = new ProcessedTitlesStorage
        {
            RowKey = System.Guid.NewGuid().ToString("N"),
            PartitionKey = "feed-processed",
            Title = title
        }.AddEtag();

        return storeTitle;
    }
}