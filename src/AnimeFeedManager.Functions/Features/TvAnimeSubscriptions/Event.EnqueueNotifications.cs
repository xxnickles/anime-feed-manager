using System.Collections.Immutable;
using AnimeFeedManager.Application;
using AnimeFeedManager.Application.Notifications.Queries;
using AnimeFeedManager.Application.TvSubscriptions.Queries;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

public class NotificationsMessages
{
    [QueueOutput(QueueNames.ProcessedTitles, Connection = "AzureWebJobsStorage")]
    public IEnumerable<string>? ProcessedTitles { get; set; }

    [QueueOutput(QueueNames.Notifications, Connection = "AzureWebJobsStorage")]
    public IEnumerable<string>? Notifications { get; set; }
}

public class EnqueueNotifications
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnqueueNotifications> _logger;

    public EnqueueNotifications(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<EnqueueNotifications>();
    }

    [Function("EnqueueNotifications")]
    public async Task<NotificationsMessages> Run(
        [QueueTrigger(QueueNames.SubscribersToProcess, Connection = "AzureWebJobsStorage")]
        string subscriber
    )
    {
        var notifications = await _mediator.Send(new GetSubscriptionsQry(subscriber))
            .BindAsync(ProcessSubscriptions);

        return notifications.Match(
            process =>
            {
                var (notificationList, titles) = process;
                return new NotificationsMessages
                {
                    Notifications = notificationList.Select(Serializer.ToJson),
                    ProcessedTitles = titles.Select(Serializer.ToJson)
                };
            },
            e =>
            {
                _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                return new NotificationsMessages();
            });
    }

    private Task<Either<DomainError, (ImmutableList<Notification> notifications, ImmutableList<NotifiedTitle> titles)>>
        ProcessSubscriptions(ImmutableList<SubscriptionCollection> subscriptionsToProcess)
    {
        return _mediator.Send(new GetEmailNotificationsQry(subscriptionsToProcess))
            .MapAsync(notifications => (notifications, Map(notifications)));
    }

    private static ImmutableList<NotifiedTitle> Map(ImmutableList<Notification> source)
    {
        return source
            .SelectMany(
                n => n.Feeds.Select(f => new NotifiedTitle(n.Subscriber, f.Title)))
            .ToImmutableList();
    }
}