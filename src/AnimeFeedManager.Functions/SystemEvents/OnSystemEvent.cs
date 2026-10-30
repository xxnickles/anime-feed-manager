using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.SystemEvents.UpdateProcess;
using AnimeFeedManager.Web.BlazorComponents;
using AnimeFeedManager.Web.BlazorComponents.SignalRContent;

namespace AnimeFeedManager.Functions.SystemEvents;

public class OnSystemEvent
{
    private static readonly ActivitySource Source = new(Telemetry.SystemEventsUpdateSource);

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
        FunctionContext functionContext,
        CancellationToken cancellationToken)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(SystemEventsTelemetry.UpdateSpan);
        return await SystemEventUpdate.StartProcess(message)
            .StoreEvent(_tableClientFactory.TableStorageEventUpdater(), cancellationToken)
            .PrepareUiNotification()
            .Bind(result =>
            {
                var audience = LiveAudience.From(result.Event.Consumer);
                if (!result.NotifyUi || audience is NoAudience)
                    return Task.FromResult(Result<SignalRMessageAction?>.Success(null));

                return NotificationFactory.GenerateNotificationContent(result.Event,
                        EventPayloadDeserializer.Deserialize, _blazorRenderer)
                    .Map(content => LiveMessages.For(audience, ServerNotifications.AlertNotifications, content));
            })
            .MarkActivityErroredOnError()
            .FlushLogs(_logger)
            .MatchToValue(r => r, _ => null);
    }
}