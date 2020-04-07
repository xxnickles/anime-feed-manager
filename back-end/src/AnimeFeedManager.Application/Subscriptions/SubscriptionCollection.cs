using System.Collections.Generic;

namespace AnimeFeedManager.Application.Subscriptions
{
    public class SubscriptionCollection
    {
        public string SubscriptionId { get; }
        public IEnumerable<string> SubscribedAnimes { get; }

        public SubscriptionCollection(string subscriptionId, IEnumerable<string> subscribedAnimes)
        {
            SubscriptionId = subscriptionId;
            SubscribedAnimes = subscribedAnimes;
        }
    }
}
