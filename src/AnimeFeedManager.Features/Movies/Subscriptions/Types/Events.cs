using AnimeFeedManager.Common.Domain.Events;

namespace AnimeFeedManager.Features.Movies.Subscriptions.Types;

public record MoviesCheckFeedMatchesEvent(string UserEmail, string UserId, string PartitionKey) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "movie-feed-matches-process";
}

public record CompleteMovieSubscriptionEvent(MoviesSubscriptionStorage MoviesInformation) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "movie-subscription-completer";
}