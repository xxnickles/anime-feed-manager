using System.ComponentModel;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Types;

public readonly record struct Year : IComparable<Year>
{
    [DefaultValue(2000)] public readonly ushort Value;

    private Year(int value)
    {
        if (!NumberIsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value));
        Value = (ushort) value;
    }


    public static Year FromNumber(int value) => new Year(value);

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

public static class YearExtensions
{
    public static Validation<Year> ParseAsYear(this int value) =>
        Year.NumberIsValid(value)
            ? Validation<Year>.Valid(Year.FromNumber(value))
            : Validation<Year>.Invalid(
                DomainValidationError.Create<Year>($"'{value}' is not a valid year")
                .ToErrors());
}