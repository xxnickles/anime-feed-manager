using System;
using Azure;
using Azure.Data.Tables;

namespace AnimeFeedManager.Storage.Domain;

public class TitlesStorage : ITableEntity
{
    public string? Titles { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}