using System.Collections.Generic;
using AnimeFeedManager.Application.Feed.Queries;
using AnimeFeedManager.Application.Notifications;
using AnimeFeedManager.Application.Notifications.Queries;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Functions.Models;
using LanguageExt;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class NotificationsMessages
{
    [QueueOutput(QueueNames.ProcessedTitles)]
    public IEnumerable<string>? ProcessedTitles { get; set; }
    [QueueOutput(QueueNames.Notifications)]
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
    [StorageAccount("AzureWebJobsStorage")]
    public async Task<NotificationsMessages> Run(
        [TimerTrigger("0 0 * * * *")] TimerInfo timer
        )
    {
        var notifications = await _mediator.Send(new GetLatestFeed(Resolution.Hd))
            .BindAsync(ProcessFeed);

        return notifications.Match(
            (process) =>
            {
                var (notificationList, titles) = process;
                return new NotificationsMessages
                {
                    Notifications = notificationList.Select(n => JsonSerializer.Serialize(n)),
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
        ProcessFeed(ImmutableList<Core.Domain.FeedInfo> feedInfo)
    {
        var titles = feedInfo.Select(t => OptionUtils.UnpackOption(t.FeedTitle.Value, string.Empty)).ToImmutableList();

        return _mediator.Send(new GetNotifications(feedInfo)).MapAsync(notifications => (notifications, titles));
    }

}