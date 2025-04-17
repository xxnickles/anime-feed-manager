namespace AnimeFeedManager.Old.Common.Domain.Types;

public readonly struct ShortSeriesStatus : IEquatable<ShortSeriesStatus>
{
    private const string ProcessedValue = "PROCESSED";
    private const string NotProcessedValue = "NOTPROCESSED";
    private const string NoFeedFoundValue = "NOFEEDFOUND";
    private const string NotAvailableValue = "NOTAVAILABLE";
    private const string SkipFromProcessValue = "SKIPFROMPROCESS";
    
    private readonly string _value;

    private ShortSeriesStatus(string value)
    {
        _value = value;
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(ShortSeriesStatus status) => status._value;
    
    public static explicit operator ShortSeriesStatus(string? status) => status switch
    {
        ProcessedValue => Processed,
        NoFeedFoundValue => NotFeedFound,
        NotAvailableValue => NotAvailable,
        SkipFromProcessValue => SkipFromProcess,
        _ => NotProcessed
    };

    public static ShortSeriesStatus Processed = new(ProcessedValue);
    public static ShortSeriesStatus NotProcessed = new(NotProcessedValue);
    public static ShortSeriesStatus NotFeedFound = new(NoFeedFoundValue);
    public static ShortSeriesStatus NotAvailable = new(NotAvailableValue);
    public static ShortSeriesStatus SkipFromProcess = new(SkipFromProcessValue);

    public bool Equals(ShortSeriesStatus other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ShortSeriesStatus other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(ShortSeriesStatus left, ShortSeriesStatus right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShortSeriesStatus left, ShortSeriesStatus right)
    {
        return !(left == right);
    }
}