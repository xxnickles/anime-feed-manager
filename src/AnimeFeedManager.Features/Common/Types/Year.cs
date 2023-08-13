namespace AnimeFeedManager.Features.Common.Types;

public readonly record struct Year : IComparable<Year>
{
    public readonly Option<ushort> Value;

    private Year(int value)
    {
        Value = IsValid(value) ? Some((ushort)value) : None;
    }

    public static Year FromNumber(int value) => new (value);

    public static bool IsValid(int value) => value >= 2000 && value <= DateTime.Now.Year + 1;
    
    public static implicit operator int(Year year) => year.Value.Match(
        v => v,
        () => 0);

    public int CompareTo(Year other)
    {
        return Value.CompareTo(other.Value);
    }
}