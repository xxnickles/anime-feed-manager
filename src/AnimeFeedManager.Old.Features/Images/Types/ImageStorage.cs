namespace AnimeFeedManager.Old.Features.Images.Types;

public sealed class ImageStorage : ITableEntity
{
    public string? ImageUrl { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}