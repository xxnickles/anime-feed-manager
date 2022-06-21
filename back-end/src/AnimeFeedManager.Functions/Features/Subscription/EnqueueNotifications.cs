using AnimeFeedManager.Application.Feed.Queries;
using AnimeFeedManager.Application.Notifications;
using AnimeFeedManager.Application.Notifications.Queries;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using LanguageExt;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class EnqueueNotifications
{
    private readonly IMediator _mediator;
    public EnqueueNotifications(IMediator mediator) => _mediator = mediator;

    [FunctionName("EnqueueNotifications")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [TimerTrigger("0 0 * * * *")] TimerInfo timer,
        [Queue(QueueNames.ProcessedTitles)] IAsyncCollector<string> processedTitlesQueueCollector,
        [Queue(QueueNames.Notifications)] IAsyncCollector<Notification> notificationsQueueCollector,
        ILogger log)
    {
        var notifications = await _mediator.Send(new GetLatestFeed(Resolution.Hd))
            .BindAsync(ProcessFeed);

        notifications.Match(
            (process) =>
            {
                var (notificationList, titles) = process;
                QueueStorage.StoreInQueue(notificationList, notificationsQueueCollector, log,
                    x => $"Queueing subscriptions for {x.Subscriber}");

                QueueStorage.StoreInQueue(titles, processedTitlesQueueCollector, log,
                    x => $"Queueing processed title {x}");
            },
            e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
        );
    }

    private Task<Either<DomainError, (ImmutableList<Notification> notifications, ImmutableList<string> titles)>>
        ProcessFeed(ImmutableList<Core.Domain.FeedInfo> feedInfo)
    {
        var titles = feedInfo.Select(t => OptionUtils.UnpackOption(t.FeedTitle.Value, string.Empty)).ToImmutableList();

        return _mediator.Send(new GetNotifications(feedInfo)).MapAsync(notifications => (notifications, titles));
    }

}