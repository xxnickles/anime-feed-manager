namespace AnimeFeedManager.Old.Common.Types;

public record NoEmptyString
{
    private readonly string _value;

    protected NoEmptyString(string value)
    {
        _value = value;
    }

    public static Option<NoEmptyString> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new NoEmptyString(value)),
        false => None
    };

    public override string ToString()
    {
        return _value;
    }
    public static implicit operator string(NoEmptyString value) => value._value;
   
}

