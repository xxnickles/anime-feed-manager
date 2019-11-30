using Microsoft.Azure.Cosmos.Table;

namespace AnimeFeedManager.Storage.Domain
{
    public class ImageStorage : TableEntity
    {
        public string? ImageUrl { get; set; }
    }
}