using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Storage.Infrastructure;

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
            _ => throw new ArgumentException($"Type '{type.Name}' has not an associated queue")
        };
    }
} 