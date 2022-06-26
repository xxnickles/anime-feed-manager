using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes;

public readonly struct Season : IComparable<Season>, IEquatable<Season>
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

    public bool Equals(Season other)
    {
        return other.Value == Value;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static Season Spring = new(SpringValue);
    public static Season Summer = new(SummerValue);
    public static Season Fall = new(FallValue);
    public static Season Winter = new(WinterValue);

    public static Season FromString(string? val) => val != null ? val.ToLowerInvariant() switch
    {
        SpringValue => Spring,
        SummerValue => Summer,
        FallValue => Fall,
        FallAlternativeValue => Fall,
        WinterValue => Winter,
        _ => Spring
    } : Spring;

    public static Option<Season> TryCreateFromString(string val)
    {
        return val switch
        {
            SpringValue => Some(Spring),
            SummerValue => Some(Summer),
            FallValue => Some(Fall),
            FallAlternativeValue => Some(Fall),
            WinterValue => Some(Winter),
            _ => None
        };
    }

    // Operators
    public static bool operator >(Season a, Season b) => GetSeasonWeight(a) > GetSeasonWeight(b);
    public static bool operator <(Season a, Season b) => GetSeasonWeight(a) < GetSeasonWeight(b);

    private static byte GetSeasonWeight(Season season) => season.Value switch
    {
        WinterValue => 0,
        SpringValue => 1,
        SummerValue => 2,
        FallValue => 3,
        _ => 0
    };

}