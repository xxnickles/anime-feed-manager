using AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;
using AnimeFeedManager.Web.BlazorComponents.Toast;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent;

public delegate Task<Result<string>> NotificationGenerator(SystemEvent notification,
    PayloadDeserializer payloadDeserializer, BlazorRenderer renderer);

public static class NotificationFactory
{
    public static Task<Result<string>> GenerateNotificationContent(
        SystemEvent notification,
        PayloadDeserializer payloadDeserializer,
        BlazorRenderer renderer)
    {
        return payloadDeserializer(notification)
            .Bind(data => ContentGenerator.GetContent(data.Instance))
            .Map(content => renderer.RenderComponent<ClosableNotification>(new Dictionary<string, object?>
            {
                {nameof(ClosableNotification.Type), Helpers.Map(notification.Type)},
                {nameof(ClosableNotification.Title), content.Title},
                {nameof(ClosableNotification.Message), content.Message}
            }));
    }
}