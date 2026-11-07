namespace AnimeFeedManager.Features.SystemEvents;

/// <summary>Who receives a live (SignalR) message.</summary>
public abstract record LiveAudience
{
    /// <summary>SignalR user group holding admins; membership is set when a connection is negotiated.</summary>
    public const string AdminGroup = "admin";

    public static LiveAudience From(TargetConsumer consumer) => consumer.ToString() switch
    {
        null or "" or TargetConsumer.NoneValue => new NoAudience(),
        TargetConsumer.EverybodyValue => new EveryConnection(),
        TargetConsumer.AdminValue => new GroupAudience(AdminGroup),
        var userId => new UserAudience(userId)
    };
}

public sealed record EveryConnection : LiveAudience;

public sealed record GroupAudience(string Group) : LiveAudience;

public sealed record UserAudience(string UserId) : LiveAudience;

/// <summary>Sends nothing, so an unset consumer never widens into a broadcast.</summary>
public sealed record NoAudience : LiveAudience;
