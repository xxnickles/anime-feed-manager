using System.Threading.Tasks;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class AddProcessedTitle
{
    private readonly ILogger<AddProcessedTitle> _logger;

    public AddProcessedTitle(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AddProcessedTitle>();
    }

    [Function("AddProcessedTitle")]
    public async Task Run(
        [QueueTrigger(QueueNames.ProcessedTitles, Connection = "AzureWebJobsStorage")]
        string title)
    {
        _logger.LogInformation("Saving {Title}", title);
        var storeTitle = new ProcessedTitlesStorage
        {
            RowKey = System.Guid.NewGuid().ToString("N"),
            PartitionKey = "feed-processed",
            Title = title
        }.AddEtag();

        await client.AddEntityAsync(storeTitle);
        
    }
}