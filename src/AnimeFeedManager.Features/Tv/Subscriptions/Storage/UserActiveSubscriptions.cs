namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage;

public record ActiveSubscription(
    string SeriesFeedTitle, 
    string[] NotifiedEpisodes);

public record UserActiveSubscriptions(
    string UserId,
    string UserEmail,
    ActiveSubscription[] Subscriptions);
    