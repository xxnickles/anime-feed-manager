namespace AnimeFeedManager.Features.Domain.Types;

public record struct Subscription(Email Subscriber, string AnimeId);
    
public record InterestedSeries(Email Subscriber, string AnimeId);

public record User(string Id, Email Email);

