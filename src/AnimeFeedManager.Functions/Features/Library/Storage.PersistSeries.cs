﻿using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class PersistSeries
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersistSeries> _logger;

    public PersistSeries(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<PersistSeries>();
    }


    [Function("PersistSeries")]
    public async Task Run(
        [QueueTrigger(QueueNames.AnimeLibrary, Connection = "AzureWebJobsStorage")] AnimeInfoStorage animeInfo
        )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", animeInfo.Title);
        var command = new MergeAnimeInfoCmd(animeInfo);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{AnimeInfoTitle} has been stored", animeInfo.Title),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}