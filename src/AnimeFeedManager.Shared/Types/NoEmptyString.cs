using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Types;

[JsonConverter(typeof(NoEmptyStringConverter))]
public record NoEmptyString
{
    private readonly string _value;

    protected NoEmptyString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));
        _value = value;
    }

    public static implicit operator string(NoEmptyString noEmpty) => noEmpty._value;

    public override string ToString()
    {
        return _value;
    }

    public static NoEmptyString FromString(string value) => new(value);
}

public static class NoEmptyStringExtensions
{
    public static Validation<NoEmptyString> ParseAsNonEmpty(this string value,
        [CallerArgumentExpression(nameof(value))] string propertyName = ""
    ) => !string.IsNullOrWhiteSpace(value)
        ? Validation<NoEmptyString>.Valid(NoEmptyString.FromString(value))
        : Validation<NoEmptyString>.Invalid(
            DomainValidationError.Create(propertyName, "Value cannot be empty")
                .ToErrors());
}

public class NoEmptyStringConverter : JsonConverter<NoEmptyString>
{
    public override NoEmptyString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var value = reader.GetString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    return NoEmptyString.FromString(value);
                throw new JsonException("Value cannot be empty");
            case JsonTokenType.Null:
            case JsonTokenType.None:
            case JsonTokenType.StartObject:
            case JsonTokenType.EndObject:
            case JsonTokenType.StartArray:
            case JsonTokenType.EndArray:
            case JsonTokenType.PropertyName:
            case JsonTokenType.Comment:
            case JsonTokenType.Number:
            case JsonTokenType.True:
            case JsonTokenType.False:
            default:
                throw new JsonException();
        }
    }

    public override void Write(Utf8JsonWriter writer, NoEmptyString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}