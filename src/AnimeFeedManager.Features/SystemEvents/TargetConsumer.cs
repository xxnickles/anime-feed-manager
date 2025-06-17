namespace AnimeFeedManager.Features.SystemEvents;

[JsonConverter(typeof(TargetConsumerConverter))]
public readonly struct TargetConsumer
{
    private readonly string _value;

    private const string AdminValue = "Admin";
    private const string EverybodyValue = "All";

    // Used as default for serialization
    private const string NoneValue = "None";


    private TargetConsumer(string value)
    {
        _value = value;
    }

    public override string ToString() => _value;

    public static implicit operator string(TargetConsumer targetConsumer) => targetConsumer._value;

    public static TargetConsumer Admin() => new(AdminValue);
    public static TargetConsumer None() => new(NoneValue);
    public static TargetConsumer Everybody() => new(EverybodyValue);
    public static TargetConsumer User(string id) => new(id);

    public static TargetConsumer FromString(string value) => value switch
    {
        AdminValue => Admin(),
        EverybodyValue => Everybody(),
        _ when !string.IsNullOrWhiteSpace(value) => User(value),
        _ => None()
    };
}

public class TargetConsumerConverter : JsonConverter<TargetConsumer>
{
    public override TargetConsumer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();

        return TargetConsumer.FromString(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, TargetConsumer value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}