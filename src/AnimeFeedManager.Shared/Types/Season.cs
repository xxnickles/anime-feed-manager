using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Types;

[JsonConverter(typeof(SeasonJsonConverter))]
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

    public static Season Spring() => new(SpringValue);
    public static Season Summer() => new(SummerValue);
    public static Season Fall() => new(FallValue);
    public static Season Winter() => new(WinterValue);
    
    public static Season[] All() => [Spring(), Summer(), Fall(), Winter()];

    public static Season Current => DateTime.UtcNow.Month switch
    {
        1 or 2 or 3 => Winter(),    // January-March
        4 or 5 or 6 => Spring(),    // April-June  
        7 or 8 or 9 => Summer(),    // July-September
        10 or 11 or 12 => Fall(),   // October-December
        _ => throw new UnreachableException("Invalid month")
    };


    public static Season FromString(string? val) => val?.ToLowerInvariant() switch
    {
        SpringValue => Spring(),
        SummerValue => Summer(),
        FallValue => Fall(),
        FallAlternativeValue => Fall(),
        WinterValue => Winter(),
        _ => throw new ArgumentException($"'{val}' is an invalid season")
    };


    public static bool IsValid(string val) => val.ToLowerInvariant() switch
    {
        SpringValue => true,
        SummerValue => true,
        FallValue => true,
        FallAlternativeValue => true,
        WinterValue => true,
        _ => false
    };


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

public class SeasonJsonConverter : JsonConverter<Season>
{
    public override Season Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();
        var seasonValue = reader.GetString() ?? string.Empty;
        if (!Season.IsValid(seasonValue))
            throw new JsonException($"Expected a value string for Season type, but got '{seasonValue}' instead.");
        return Season.FromString(seasonValue);
    }

    public override void Write(Utf8JsonWriter writer, Season value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}