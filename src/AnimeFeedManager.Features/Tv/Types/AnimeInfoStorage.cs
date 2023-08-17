namespace AnimeFeedManager.Features.Tv.Types;

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

internal readonly struct SeriesStatus : IEquatable<SeriesStatus>
{
    private const string CompletedValue = "COMPLETED";
    private const string NotAvailableValue = "NOTAVAILABLE";
    private const string OngoingValue = "ONGOING";
    
    private readonly string _value;

    private SeriesStatus(string value)
    {
        _value = value;
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(SeriesStatus status) => status._value;
    public static explicit operator SeriesStatus(string status) => status switch
    {
        CompletedValue => Completed,
        OngoingValue => Ongoing,
        _ => NotAvailable,
        
    };

    public static SeriesStatus Completed = new(CompletedValue);
    public static SeriesStatus NotAvailable = new(NotAvailableValue);
    public static SeriesStatus Ongoing = new(OngoingValue);


    public bool Equals(SeriesStatus other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is SeriesStatus other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}


