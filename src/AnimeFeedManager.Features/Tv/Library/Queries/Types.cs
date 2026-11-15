namespace AnimeFeedManager.Features.Tv.Library.Queries;

public closed record UserTvSeries(TvSeries TvSeries);

/// <summary>A series as a signed-out visitor sees it.</summary>
public closed record PublicSeries(TvSeries TvSeries) : UserTvSeries(TvSeries)
{
    public sealed record Available(TvSeries TvSeries) : PublicSeries(TvSeries);

    public sealed record NotAvailable(TvSeries TvSeries) : PublicSeries(TvSeries);

    public sealed record Completed(TvSeries TvSeries) : PublicSeries(TvSeries);
}

/// <summary>A series as a signed-in user sees it; <paramref name="User"/> decides the admin actions.</summary>
public closed record SubscriberSeries(AuthenticatedUser User, TvSeries TvSeries) : UserTvSeries(TvSeries)
{
    public sealed record AvailableForSubscription(AuthenticatedUser User, TvSeries TvSeries) : SubscriberSeries(User, TvSeries);

    public sealed record AvailableForFuture(AuthenticatedUser User, TvSeries TvSeries) : SubscriberSeries(User, TvSeries);

    public sealed record Subscribed(AuthenticatedUser User, TvSeries TvSeries) : SubscriberSeries(User, TvSeries);

    public sealed record Interested(AuthenticatedUser User, TvSeries TvSeries) : SubscriberSeries(User, TvSeries);

    // Takes no user actions; IsSubscribed keeps the subscription visible to filtering.
    public sealed record Completed(AuthenticatedUser User, TvSeries TvSeries, bool IsSubscribed) : SubscriberSeries(User, TvSeries);
}
