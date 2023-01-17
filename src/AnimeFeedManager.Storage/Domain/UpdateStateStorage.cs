using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Storage.Domain;

public sealed class UpdateStateStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public int SeriesToUpdate { get; set; }
    public UpdateType Type { get; set; }
    public string? StateGroup { get; set; }
}