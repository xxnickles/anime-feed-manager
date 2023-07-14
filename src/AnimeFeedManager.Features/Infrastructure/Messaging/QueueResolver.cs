using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Domain.Notifications;

namespace AnimeFeedManager.Features.Infrastructure.Messaging;

public interface IQueueResolver
{
    string GetQueue(Type type);
}

public sealed class QueueResolver : IQueueResolver
{
    public string GetQueue(Type type)
    {
        return type.Name switch
        {
            nameof(SeasonProcessNotification) => Boxes.SeasonProcessNotifications,
            // nameof(TitlesUpdateNotification) => Boxes.TitleUpdatesNotifications,
            nameof(ImageUpdateNotification) => Boxes.ImageUpdateNotifications,
            nameof(DownloadImageEvent) => Boxes.ImageProcess,
            _ => throw new ArgumentException($"Type '{type.Name}' has not an associated queue")
        };
    }
}