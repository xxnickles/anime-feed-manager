namespace AnimeFeedManager.Features.State.Types;

public sealed class StateUpdateStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public int SeriesToUpdate { get; set; }
    public int Completed { get; set; }
    public int Errors { get; set; }
    
}