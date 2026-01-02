using AnimeFeedManager.Features.SystemEvents.Storage;
using AnimeFeedManager.Features.SystemEvents.UpdateProcess;
using AnimeFeedManager.Web.BlazorComponents;
using AnimeFeedManager.Web.BlazorComponents.SignalRContent;

namespace AnimeFeedManager.Functions.SystemEvents;

public class OnSystemEvent
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly BlazorRenderer _blazorRenderer;
    private readonly ILogger<OnSystemEvent> _logger;

    public OnSystemEvent(
        ITableClientFactory tableClientFactory,
        BlazorRenderer blazorRenderer,
        ILogger<OnSystemEvent> logger)
    {
        _tableClientFactory = tableClientFactory;
        _blazorRenderer = blazorRenderer;
        _logger = logger;
    }

    [Function(nameof(OnSystemEvent))]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = StorageRegistrationConstants.SignalRConnectionName)]
    public async Task<SignalRMessageAction?> Run(
        [QueueTrigger(SystemEvent.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        SystemEvent message,
        CancellationToken cancellationToken)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnSystemEvent));
        return await SystemEventUpdate.StartProcess(message)
            .StoreEvent(_tableClientFactory.TableStorageEventUpdater(), cancellationToken)
            .PrepareUiNotification()
            .Bind(result =>
            {
                if (!result.NotifyUi)
                    return Task.FromResult(Result<SignalRMessageAction?>.Success(null));

                return NotificationFactory.GenerateNotificationContent(result.Event,
                        EventPayloadDeserializer.Deserialize, _blazorRenderer)
                    .Map<string, SignalRMessageAction?>(CreateSignalRMessage);
            })
            .WriteLogs(_logger)
            .MatchToValue(r => r, _ => null);
    }

    private static SignalRMessageAction CreateSignalRMessage(string content) =>
        new(ServerNotifications.AlertNotifications)
        {
            Arguments = [content]
        };
}