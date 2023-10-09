namespace AnimeFeedManager.Common.Domain.Types;

public readonly struct SeriesStatus : IEquatable<SeriesStatus>
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

    public static bool operator ==(SeriesStatus left, SeriesStatus right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SeriesStatus left, SeriesStatus right)
    {
        return !(left == right);
    }
}