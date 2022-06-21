using Microsoft.Azure.Cosmos.Table;

namespace AnimeFeedManager.Storage.Domain;

public class SeasonStorage: TableEntity
{
    public string? Season { get; set; }
    public int Year { get; set; } // Azure tables only works with Int and Int64
}