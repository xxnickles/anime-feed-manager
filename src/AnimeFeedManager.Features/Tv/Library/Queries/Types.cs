namespace AnimeFeedManager.Features.Tv.Library.Queries;

public abstract record UserTvSeries(AppUser User, TvSeries TvSeries);

public record NotAvailable(AppUser User, TvSeries TvSeries) : UserTvSeries(User, TvSeries);
public record Available(AppUser User, TvSeries TvSeries) : UserTvSeries(User, TvSeries);
// Completed series take no user actions; IsSubscribed keeps the subscription visible to filtering.
public record Completed(AppUser User, TvSeries TvSeries, bool IsSubscribed) : UserTvSeries(User, TvSeries);
public record AvailableForFuture(AppUser User, TvSeries TvSeries) : UserTvSeries(User, TvSeries);
public record AvailableForSubscription(AppUser User, TvSeries TvSeries) : UserTvSeries(User, TvSeries);
public record Interested(AppUser User, TvSeries TvSeries) : UserTvSeries(User, TvSeries);
public record Subscribed(AppUser User, TvSeries TvSeries) : UserTvSeries(User, TvSeries);
