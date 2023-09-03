namespace AnimeFeedManager.Features.Common.Types;

public record NoEmptyString
{
    public readonly string Value;

    private NoEmptyString(string value)
    {
        Value = value;
    }

    public static Option<NoEmptyString> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new NoEmptyString(value)),
        false => None
    };

    public static implicit operator string(NoEmptyString value) => value.Value;
}

