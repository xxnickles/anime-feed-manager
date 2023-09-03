﻿using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.IO;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Images;

public readonly record struct ScrapNotificationImages(ImmutableList<DownloadImageEvent> Events) : INotification;

public sealed class ScrapImagesNotificationHandler : INotificationHandler<ScrapNotificationImages>
{
    private readonly ICreateState _stateCreator;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<ScrapImagesNotificationHandler> _logger;

    public ScrapImagesNotificationHandler(ICreateState stateCreator, IDomainPostman domainPostman,
        ILogger<ScrapImagesNotificationHandler> logger)
    {
        _stateCreator = stateCreator;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    public async Task Handle(ScrapNotificationImages notification, CancellationToken cancellationToken)
    {
        var results = await _stateCreator.Create(NotificationTarget.Images, notification.Events)
            .MapAsync(r => SendMessages(r, cancellationToken));

        results.Match(async r => await r,
            e => e.LogDomainError(_logger));
    }

    private async Task SendMessages(ImmutableList<StateWrap<DownloadImageEvent>> events, CancellationToken token)
    {
        var results = events.AsParallel()
            .Select(imageEvent => _domainPostman.SendMessage(imageEvent, Box.ImageProcess, token));

        try
        {
            await Task.WhenAll(results);
        }
        catch (AggregateException e)
        {
            foreach (var exception in e.InnerExceptions)
            {
                _logger.LogError(exception, "An Error has occurred when sending image information for scrapping");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An Error has occurred when sending image information for scrapping");
        }
        
    }
}