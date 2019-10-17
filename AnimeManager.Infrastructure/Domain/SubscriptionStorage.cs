using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace AnimeFeedManager.Storage.Domain
{
    public class SubscriptionStorage : TableEntity
    {
        public string? AnimeIds { get; set; }
     
    }
}
