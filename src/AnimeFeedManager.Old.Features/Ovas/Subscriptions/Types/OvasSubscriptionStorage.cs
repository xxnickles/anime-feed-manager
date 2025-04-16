﻿namespace AnimeFeedManager.Features.Ovas.Subscriptions.Types;

public sealed class OvasSubscriptionStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public DateTimeOffset? DateToNotify { get; set; }
    public ETag ETag { get; set; }
    public bool Processed { get; set; }
}