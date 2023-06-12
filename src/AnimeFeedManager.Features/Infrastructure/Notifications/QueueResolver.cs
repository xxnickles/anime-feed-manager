namespace AnimeFeedManager.Features.Infrastructure.Notifications;

public interface IQueueResolver
{
    string GetQueue(Type type);
}

// public sealed class QueueResolver : IQueueResolver
// {
//     public string GetQueue(Type type)
//     {
//         return type.Name switch
//         {
//             nameof(SeasonProcessNotification) => Boxes.SeasonProcessNotifications,
//             nameof(TitlesUpdateNotification) => Boxes.TitleUpdatesNotifications,
//             nameof(ImageUpdateNotification) => Boxes.ImageUpdateNotifications,
//             _ => throw new ArgumentException($"Type '{type.Name}' has not an associated queue")
//         };
//     }
// } 