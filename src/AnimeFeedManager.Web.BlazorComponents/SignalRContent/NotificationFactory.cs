using AnimeFeedManager.Web.BlazorComponents.Toast;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent;

public static class NotificationFactory
{
    public static Task<Result<string>> GenerateNotificationContent(
        SystemEvent notification,
        PayloadDeserializer payloadDeserializer,
        BlazorRenderer renderer)
    {
        return payloadDeserializer(notification)
            .Map(data => data.AsNotificationComponent())
            .Map(content => renderer.RenderComponent<ClosableNotification>(new Dictionary<string, object?>
            {
                {nameof(ClosableNotification.Type), Helpers.Map(notification.Type)},
                {nameof(ClosableNotification.Title), content.Title},
                {nameof(ClosableNotification.Message), content.Content},
                {nameof(ClosableNotification.CloseTime), TimeSpan.FromSeconds(8)}
            }));
    }
}