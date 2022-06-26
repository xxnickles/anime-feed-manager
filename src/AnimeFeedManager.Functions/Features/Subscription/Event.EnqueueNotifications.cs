using System.Collections.Immutable;
using AnimeFeedManager.Application.Feed.Queries;
using AnimeFeedManager.Application.Notifications;
using AnimeFeedManager.Application.Notifications.Queries;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

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
        [TimerTrigger("0 0 * * * *")] TimerInfo timer
        )
    {
        var notifications = await _mediator.Send(new GetLatestFeedQry(Resolution.Hd))
            .BindAsync(ProcessFeed);

        return notifications.Match(
            process =>
            {
                var (notificationList, titles) = process;
                return new NotificationsMessages
                {
                    Notifications = notificationList.Select(Serializer.ToJson),
                    ProcessedTitles = titles
                };
            },
            e =>
            {
                _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                return new NotificationsMessages();
            });
    }

    private Task<Either<DomainError, (ImmutableList<Notification> notifications, ImmutableList<string> titles)>>
        ProcessFeed(ImmutableList<FeedInfo> feedInfo)
    {
        var titles = feedInfo.Select(t => OptionUtils.UnpackOption(t.FeedTitle.Value, string.Empty)).ToImmutableList();

        return _mediator.Send(new GetNotificationsQry(feedInfo)).MapAsync(notifications => (notifications, titles));
    }

}