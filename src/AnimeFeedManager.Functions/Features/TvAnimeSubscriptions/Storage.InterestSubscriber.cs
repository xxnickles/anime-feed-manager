using AnimeFeedManager.Application.TvSubscriptions.Queries;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

public class GetLibraryMessages
{
    [QueueOutput(QueueNames.ToSubscribe, Connection = "AzureWebJobsStorage")] public IEnumerable<string>? ToSubscribeMessages { get; set; }

    [QueueOutput(QueueNames.DeleteInterested, Connection = "AzureWebJobsStorage")]
    public IEnumerable<string>? InterestRemoveMessages { get; set; }
}

public class InterestSubscriber
{
    private readonly IMediator _mediator;
    private readonly ILogger<InterestSubscriber> _logger;

    public InterestSubscriber(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<InterestSubscriber>();
    }

    [Function("InterestSubscriber")]
    public async Task<GetLibraryMessages> Run(
        [QueueTrigger(QueueNames.ProcessAutoSubscriber, Connection = "AzureWebJobsStorage")]
        string processResult
        )
    {
        if (processResult == ProcessResult.Ok)
        {
            _logger.LogInformation(
                "Titles were updated and checking of status process was completed, checking if there are interested series that match new titles");
            var interestedSeries = await _mediator.Send(new GetAllInterestedSeriesQry());

            var resultSet = await interestedSeries
                .MapAsync(async series =>
                {
                    var response = await _mediator.Send(new SubscribeToInterestQry(series));
                    return response.Match(
                        r =>
                        {
                            if (r.Any())
                            {
                                var toSubscribe = r.Select(s => new TvSubscriptionDto(s.UserId, s.FeedTitle));
                                var toRemoveFromInterest =
                                    r.Select(s => new TvSubscriptionDto(s.UserId, s.InterestedTitle));

                                return new GetLibraryMessages
                                {
                                    InterestRemoveMessages =
                                        toRemoveFromInterest.Select(Serializer.ToJson),
                                    ToSubscribeMessages = toSubscribe.Select(Serializer.ToJson)
                                };
                            }

                            _logger.LogInformation("Nothing to process");
                            return new GetLibraryMessages();
                        },
                        e =>
                        {
                            _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                            return new GetLibraryMessages();
                        });
                });

            return resultSet.Match(
                o => o,
                e =>
                {
                    _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                    return new GetLibraryMessages();
                }
            );
        }

        _logger.LogWarning("Title process failed, nothing to update");
        return new GetLibraryMessages();
    }
}