using Microsoft.Azure.Cosmos.Table;

namespace AnimeFeedManager.Storage.Domain
{
    public class TitlesStorage : TableEntity
    {
        public string? Titles { get; set; }
    }
}
