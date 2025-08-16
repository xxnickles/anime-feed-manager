namespace AnimeFeedManager.Features.Infrastructure.Messaging;

[JsonConverter(typeof(BoxJsonConverter))]
public readonly struct Box(string boxValue)
{
    private const string EmptyBox = "EMPTY";

    private readonly string _boxValue = boxValue;

    public bool HasNoTarget() => _boxValue == EmptyBox;

    public override string ToString()
    {
        return _boxValue;
    }

    public static implicit operator string(Box box) => box._boxValue;

    public static Box Empty() => new(EmptyBox);
}

public class BoxJsonConverter : JsonConverter<Box>
{
    public override Box Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();
        var readerStringValue = reader.GetString() ?? string.Empty;
        
        return string.IsNullOrWhiteSpace(readerStringValue) ? Box.Empty() : new Box(readerStringValue);
    }

    public override void Write(Utf8JsonWriter writer, Box value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}