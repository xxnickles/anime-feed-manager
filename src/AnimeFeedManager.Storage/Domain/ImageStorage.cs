using System;
using Azure;
using Azure.Data.Tables;

namespace AnimeFeedManager.Storage.Domain;

public class ImageStorage : ITableEntity
{
    public string? ImageUrl { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}