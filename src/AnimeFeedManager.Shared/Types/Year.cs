using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Types;

[JsonConverter(typeof(YearJsonConverter))]
public readonly record struct Year : IComparable<Year>
{
    [DefaultValue(2000)] public readonly ushort Value;

    private Year(int value)
    {
        if (!NumberIsValid(value))
            throw new ArgumentOutOfRangeException(nameof(value));
        Value = (ushort) value;
    }


    public static Year FromNumber(int value) => new(value);

    public static bool NumberIsValid(int value) => value >= MinYear && value <= MaxYear;

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
    
    public const int MinYear = 2000;
    public static readonly int MaxYear =  DateTime.UtcNow.Year + 1;
    
    public static Year Current => FromNumber(DateTime.UtcNow.Year);
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

public class YearJsonConverter : JsonConverter<Year>
{
    public override Year Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                var yearValue = reader.GetInt32();
                if (Year.NumberIsValid(yearValue))
                    return Year.FromNumber(yearValue);

                throw new JsonException(
                    $"Year value {yearValue} is outside the valid range. Year must be between 2000 and {DateTime.Now.Year + 1}.");

            case JsonTokenType.None:
            case JsonTokenType.StartObject:
            case JsonTokenType.EndObject:
            case JsonTokenType.StartArray:
            case JsonTokenType.EndArray:
            case JsonTokenType.PropertyName:
            case JsonTokenType.Comment:
            case JsonTokenType.String:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.Null:
            default:
                throw new JsonException();
        }
      
    }

    public override void Write(Utf8JsonWriter writer, Year value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}