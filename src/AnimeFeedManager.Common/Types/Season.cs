using AnimeFeedManager.Common.Results;

namespace AnimeFeedManager.Common.Types;

public readonly record struct Season : IComparable<Season>
{
    public readonly string Value;
    private const string SpringValue = "spring";
    private const string SummerValue = "summer";
    private const string FallValue = "fall";
    private const string FallAlternativeValue = "autumn"; // Only for parsing due to AniDb
    private const string WinterValue = "winter";

    private Season(string value)
    {
        Value = value;
    }

    public int CompareTo(Season other)
    {
        return this > other ? 1 : -1;
    }

    public override string ToString()
    {
        return Value;
    }
    
    public string ToAlternativeString()
    {
        return Value == FallValue ? FallAlternativeValue : Value;
    }

    public bool Equals(Season other)
    {
        return other.Value == Value;
    }

    public override int GetHashCode()
    {
        // When you have multiple fields HashCode.Combine can be used
        return Value.GetHashCode();
    }

    public static Season Spring = new(SpringValue);
    public static Season Summer = new(SummerValue);
    public static Season Fall = new(FallValue);
    public static Season Winter = new(WinterValue);


    public static Season FromString(string? val) =>  val?.ToLowerInvariant() switch
    {
        SpringValue => Spring,
        SummerValue => Summer,
        FallValue => Fall,
        FallAlternativeValue => Fall,
        WinterValue => Winter,
        _ => throw new ArgumentException($"'{val}' is an invalid season")
    };
    
    
    public static bool IsValid(string val) =>  val.ToLowerInvariant() switch
    {
        SpringValue => true,
        SummerValue => true,
        FallValue => true,
        FallAlternativeValue => true,
        WinterValue => true,
        _ => false
    } ;


    // Operators
    public static bool operator >(Season a, Season b) => GetSeasonWeight(a) > GetSeasonWeight(b);
    public static bool operator <(Season a, Season b) => GetSeasonWeight(a) < GetSeasonWeight(b);
    
    public static implicit operator string(Season season) => season.Value;

    private static byte GetSeasonWeight(Season season) => season.Value switch
    {
        WinterValue => 0,
        SpringValue => 1,
        SummerValue => 2,
        FallValue => 3,
        _ => 0
    };

}

public static class SeasonExtensions 
{
    public static Validation<Season> ParseAsSeason(this string value) =>
            Season.IsValid(value)
                ? Validation<Season>.Valid(Season.FromString(value))
                : Validation<Season>.Invalid(
                    DomainValidationError.Create<Season>($"'{value}' is not a valid Season")
                    .ToErrors());
}