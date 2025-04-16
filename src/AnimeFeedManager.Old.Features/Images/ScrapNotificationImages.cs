﻿using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Images;

public sealed class ScrapImagesNotificationHandler(
    IStateCreator stateCreatorCreator,
    IDomainPostman domainPostman,
    ILogger<ScrapImagesNotificationHandler> logger)
{
    public async Task Handle(ScrapImagesRequest notification, CancellationToken cancellationToken)
    {
        var results = await stateCreatorCreator.Create(NotificationTarget.Images, notification.Events, new Box(DownloadImageEvent.TargetQueue))
            .MapAsync(r => SendMessages(r, cancellationToken));

        results.Match(async r => await r,
            e => e.LogError(logger));
    }

    private async Task SendMessages(ImmutableList<StateWrap<DownloadImageEvent>> events, CancellationToken token)
    {
        var results = events.AsParallel()
            .Select(imageEvent => domainPostman.SendMessage(imageEvent, token));

        var processResults = await Task.WhenAll(results);

        foreach (var processResult in processResults)
        {
            processResult.Match(
                _ => { },
                error => error.LogError(logger)
            );
        }
    }
}