using AnimeFeedManager.Application.Feed.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class AddProcessedTitle
{
    private readonly IMediator _mediator;
    private readonly ILogger<AddProcessedTitle> _logger;

    public AddProcessedTitle(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<AddProcessedTitle>();
    }

    [Function("AddProcessedTitle")]
    public async Task Run(
        [QueueTrigger(QueueNames.ProcessedTitles, Connection = "AzureWebJobsStorage")]
        NotifiedTitle title)
    {
        _logger.LogInformation("Saving processed titles {Title} for {Subscriber}", title.Title, title.Subscriber);
        var storeTitle = new ProcessedTitlesStorage
        {
            RowKey = title.Title,
            PartitionKey = title.Subscriber
        };

        var result = await _mediator.Send(new AddProcessedTitleCmd(storeTitle));
        result.Match(
            _ => _logger.LogInformation("{Title} has been added to processed titles", title),
            e => _logger.LogError("An error occurred while storing '{Title}': {Error}", title, e.ToString())
        );
    }
}