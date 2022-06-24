using System.Threading.Tasks;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class AddProcessedTitle
{
    [FunctionName("AddProcessedTitle")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [QueueTrigger(QueueNames.ProcessedTitles)]
        string title,
        [Table(Tables.ProcessedTitles)] TableClient client,
        ILogger log)
    {
        log.LogInformation("Saving {Title}", title);
        var storeTitle = new ProcessedTitlesStorage
        {
            RowKey = System.Guid.NewGuid().ToString("N"),
            PartitionKey = "feed-processed",
            Title = title
        }.AddEtag();

        await client.AddEntityAsync(storeTitle);
        
    }
}