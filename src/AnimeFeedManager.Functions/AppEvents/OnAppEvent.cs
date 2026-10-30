namespace AnimeFeedManager.Functions.AppEvents;

public class OnAppEvent
{
    [Function(nameof(OnAppEvent))]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = StorageRegistrationConstants.SignalRConnectionName)]
    public SignalRMessageAction? Run(
        [QueueTrigger(AppEvent.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        AppEvent message,
        FunctionContext functionContext)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        return LiveMessages.For(LiveAudience.From(message.Consumer), ServerNotifications.AppEvents, message.ToEnvelope());
    }
}
