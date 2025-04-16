﻿using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Tv.Types;

public class UpdateFeedAnimeInfoStorage : ITableEntity 
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string? FeedTitle { get; set; }
    public string? Status { get; set; }
}

public class AnimeInfoStorage : ITableEntity
{
    public string? Title { get; set; }
    public string? Synopsis { get; set; }
    public string? FeedTitle { get; set; }
    public int Year { get; set; } // Azure tables only works with Int and Int64
    public string? Season { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public sealed class AnimeInfoWithImageStorage : AnimeInfoStorage
{
    public string? ImageUrl { get; set; }
}

public class AlternativeTitleStorage : ITableEntity
{
    public string? AlternativeTitle { get; set; }
    public string? OriginalTitle { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Status { get; set; } = SeriesStatus.NotAvailable;
}