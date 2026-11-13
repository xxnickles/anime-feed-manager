namespace AnimeFeedManager.Web.Features.RealTime;

public closed record RealTimeError(string Message, Exception Exception) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger =>
        logger.LogError(Exception, "{domain_error.message} {domain_error.detail}", Exception.Message, Message);

    public sealed record HubUnavailable(Exception Exception)
        : RealTimeError("The notifications hub context could not be created", Exception);

    public sealed record GroupSyncFailed(Exception Exception)
        : RealTimeError("The admin group membership could not be updated", Exception);

    public sealed record NegotiationFailed(Exception Exception)
        : RealTimeError("The SignalR connection could not be negotiated", Exception);
}
