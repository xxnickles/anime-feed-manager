namespace AnimeFeedManager.Application.Subscriptions;

public class InterestedSeriesItem
{
    public string UserId { get; }
    public string InterestedAnime { get; }

    public InterestedSeriesItem(string userId, string interestedAnime)
    {
        UserId = userId;
        InterestedAnime = interestedAnime;
    }
}