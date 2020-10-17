using Microsoft.Azure.Cosmos.Table;

namespace AnimeFeedManager.Storage.Domain
{
    public class ProcessedTitlesStorage : TableEntity
    {
        public string? Title { get; set; }
    }
}