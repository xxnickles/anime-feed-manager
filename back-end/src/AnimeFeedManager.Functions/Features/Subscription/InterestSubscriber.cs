﻿using System.Collections.Generic;
using AnimeFeedManager.Application.Subscriptions.Queries;
using AnimeFeedManager.Core.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class GetLibraryMessages
{
    [QueueOutput(QueueNames.ToSubscribe)] public IEnumerable<string>? ToSubscribeMessages { get; set; }

    [QueueOutput(QueueNames.InterestRemove)]
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
    [StorageAccount("AzureWebJobsStorage")]
    public async Task<GetLibraryMessages> Run(
        [QueueTrigger(QueueNames.ProcessAutoSubscriber)]
        string processResult
        )
    {
        if (processResult == ProcessResult.Ok)
        {
            _logger.LogInformation(
                "Titles were updated and checking of status process was completed, checking if there are interested series that match new titles");
            var interestedSeries = await _mediator.Send(new GetAllInterestedSeries());

            var resultSet = await interestedSeries
                .MapAsync(async series =>
                {
                    var response = await _mediator.Send(new SubscribeToInterest(series));
                    return response.Match(
                        r =>
                        {
                            if (r.Any())
                            {
                                var toSubscribe = r.Select(s => new SubscriptionDto(s.UserId, s.FeedTitle));
                                var toRemoveFromInterest =
                                    r.Select(s => new SubscriptionDto(s.UserId, s.InterestedTitle));

                                return new GetLibraryMessages
                                {
                                    InterestRemoveMessages =
                                        toRemoveFromInterest.Select(r => JsonSerializer.Serialize(r)),
                                    ToSubscribeMessages = toSubscribe.Select(s => JsonSerializer.Serialize(s))
                                };
                            }

                            _logger.LogInformation($"Nothing to process");
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

        _logger.LogInformation("Title process failed, nothing to update");
        return new GetLibraryMessages();
    }
}