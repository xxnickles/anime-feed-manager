namespace AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

public sealed class OvaStorage : ITableEntity
{
    public string? Title { get; set; }
    public string? Synopsis { get; set; }
    public int Year { get; set; } // Azure tables only works with Int and Int64
    public string? Season { get; set; }
    public DateTime? Date { get; set; }
    public string? ImageUrl { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}