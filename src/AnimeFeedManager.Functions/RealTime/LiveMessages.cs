namespace AnimeFeedManager.Functions.RealTime;

internal static class LiveMessages
{
    /// <summary>A SignalR message addressed to the audience; null when the audience is empty.</summary>
    internal static SignalRMessageAction? For(LiveAudience audience, string method, string argument) => audience switch
    {
        EveryConnection => new SignalRMessageAction(method) { Arguments = [argument] },
        GroupAudience group => new SignalRMessageAction(method) { Arguments = [argument], GroupName = group.Group },
        UserAudience user => new SignalRMessageAction(method) { Arguments = [argument], UserId = user.UserId },
        _ => null
    };
}
