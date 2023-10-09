using Email = AnimeFeedManager.Common.Types.Email;

namespace AnimeFeedManager.Common.Domain.Types
{
    public record struct Subscription(Email Subscriber, string AnimeId);

    public record InterestedSeries(Email Subscriber, string AnimeId);
}