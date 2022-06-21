namespace AnimeFeedManager.Application.Subscriptions;

public class InterestedToSubscription
{
    public string UserId { get; }
    public string InterestedTitle { get; }
    public string FeedTitle { get; }

    public InterestedToSubscription(string userId, string interestedTitle, string feedTitle)
    {
        UserId = userId;
        InterestedTitle = interestedTitle;
        FeedTitle = feedTitle;
    }
}