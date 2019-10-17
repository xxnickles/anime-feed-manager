using System.Collections.Generic;
using AnimeFeedManager.Storage.Domain;
using System.Linq;

namespace AnimeFeedManager.Application.Subscriptions
{
    internal class Mapper
    {
        internal static SubscriptionCollection ProjectToSubscriptionCollection(SubscriptionStorage storage) =>
            new SubscriptionCollection(storage.RowKey, ProjectIds(storage.AnimeIds));

        private static IEnumerable<string> ProjectIds(string? value) =>
            !string.IsNullOrEmpty(value) ? value.Split(',') : Enumerable.Empty<string>();

    }
}
