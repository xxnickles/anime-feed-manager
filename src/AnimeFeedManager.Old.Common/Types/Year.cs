using System.ComponentModel;

namespace AnimeFeedManager.Old.Common.Types;

public readonly record struct Year : IComparable<Year>
{
    [DefaultValue(2000)] public readonly ushort Value;

    private Year(int value)
    {
        Value = (ushort)value;
    }

    public static Option<Year> TryFromNumber(int value) => NumberIsValid(value) ? Some(new Year(value)) : None;

    public static Year FromNumber(int value) => NumberIsValid(value) ? new Year(value) : default;

    public static bool NumberIsValid(int value) => value >= 2000 && value <= DateTime.Now.Year + 1;

    public static implicit operator int(Year year) => year.Value;
    public static implicit operator ushort(Year year) => year.Value;

    public int CompareTo(Year other)
    {
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}