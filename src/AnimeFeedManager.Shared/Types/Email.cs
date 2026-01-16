using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AnimeFeedManager.Shared.Results;

namespace AnimeFeedManager.Shared.Types;

[JsonConverter(typeof(EmailJsonConverter))]
public partial record Email
{
    private readonly string _value;

    private Email(string value)
    {
        if(!IsEmail(value))
            throw new ArgumentException($"'{value}' is an invalid email", nameof(value));
        _value = value;
    }

    public static bool IsEmail(string value) => LocalRegex().Match(value).Success;
    public static Email FromString(string value) => new(value);

    [GeneratedRegex(
        @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?")]
    private static partial Regex LocalRegex();

    public static implicit operator string(Email email) => email._value;

    public override string ToString()
    {
        return _value;
    }
}

public static class EmailExtensions
{
    public static Validation<Email> ParseAsEmail(this string value) => Email.IsEmail(value)
        ? Validation<Email>.Valid(Email.FromString(value))
        : Validation<Email>.Invalid($"'{value}' is not a valid email");
}


public class EmailJsonConverter : JsonConverter<Email>
{
    public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var emailValue = reader.GetString() ?? string.Empty;
                if(Email.IsEmail(emailValue))
                    return Email.FromString(emailValue);
                throw new JsonException($"'{emailValue}' is not a valid email");
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

    public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
