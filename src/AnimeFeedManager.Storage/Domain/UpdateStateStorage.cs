using AnimeFeedManager.Common;

namespace AnimeFeedManager.Storage.Domain;

public sealed class UpdateStateStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public SeriesType UpdateType { get; set; }
    public int SeriesToUpdate { get; set; }
    public int Errors { get; set; }
    public int Completed { get; set; }
}