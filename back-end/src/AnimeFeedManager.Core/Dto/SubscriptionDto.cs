namespace AnimeFeedManager.Core.Dto;

public class SubscriptionDto
{
    public SubscriptionDto(string userId, string series)
    {
        UserId = userId;
        Series = series;
    }

    public string UserId { get; set; }
    public string Series { get; set; }
}