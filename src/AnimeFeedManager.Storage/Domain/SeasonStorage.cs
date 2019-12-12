using Microsoft.Azure.Cosmos.Table;

namespace AnimeFeedManager.Storage.Domain
{
    public class SeasonStorage: TableEntity
    {
        public string? Season { get; set; }
        public ushort Year { get; set; }
    }
}
