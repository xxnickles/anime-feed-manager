using Microsoft.Azure.Cosmos.Table;
using System;

namespace AnimeFeedManager.Storage.Domain;

public class AnimeInfoStorage : TableEntity
{
    public string? Title { get; set; }
    public string? Synopsis { get; set; }
    public string? FeedTitle { get; set; }
    public int Year { get; set; } // Azure tables only works with Int and Int64
    public string? Season { get; set; }
    public DateTime? Date { get; set; }
    public bool Completed { get; set; }
}

public class AnimeInfoWithImageStorage : AnimeInfoStorage
{
    public string? ImageUrl { get; set; }
}