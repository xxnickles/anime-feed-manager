namespace AnimeFeedManager.Features.Tv.FeedManager.Storage;

public static class BindingOrigin
{
    public const string Schedule = "schedule";
    public const string Manual = "manual";
}

/// <summary>Written only with Replace, so an absent nullable clears instead of merging.</summary>
[WithTableName(AzureTableMap.StoreTo.NyaaBindings)]
public sealed class NyaaBindingStorage : ITableEntity
{
    public string? PartitionKey { get; set; } // season key, e.g. 2026-fall
    public string? RowKey { get; set; } // series id
    public string Group { get; set; } = string.Empty;
    public string ReleaseTitle { get; set; } = string.Empty;
    public string Variant { get; set; } = string.Empty;
    public string FeedTitle { get; set; } = string.Empty;
    public bool Trusted { get; set; }
    public bool Tracking { get; set; }
    public int? Episodes { get; set; }
    public string Origin { get; set; } = BindingOrigin.Manual;
    public DateTimeOffset? Premiere { get; set; } // library Date at bind time
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
