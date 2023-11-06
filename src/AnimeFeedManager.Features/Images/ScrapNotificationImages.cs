using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.IO;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Images;

public readonly record struct ScrapNotificationImages(ImmutableList<DownloadImageEvent> Events) : INotification;

public sealed class ScrapImagesNotificationHandler(
    ICreateState stateCreator,
    IDomainPostman domainPostman,
    ILogger<ScrapImagesNotificationHandler> logger)
    : INotificationHandler<ScrapNotificationImages>
{
    public async Task Handle(ScrapNotificationImages notification, CancellationToken cancellationToken)
    {
        var results = await stateCreator.Create(NotificationTarget.Images, notification.Events)
            .MapAsync(r => SendMessages(r, cancellationToken));

        results.Match(async r => await r,
            e => e.LogDomainError(logger));
    }

    private async Task SendMessages(ImmutableList<StateWrap<DownloadImageEvent>> events, CancellationToken token)
    {
        var results = events.AsParallel()
            .Select(imageEvent => domainPostman.SendMessage(imageEvent, Box.ImageProcess, token));

        var processResults = await Task.WhenAll(results);

        foreach (var processResult in processResults)
        {
            processResult.Match(
                _ => { },
                error => error.LogDomainError(logger)
            );
        }
    }
}