namespace AnimeFeedManager.Common.Types;

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

public record SeriesTitle : NoEmptyString
{
    protected SeriesTitle(string value) : base(value)
    {
    }

    public new static Option<SeriesTitle> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new SeriesTitle(value)),
        false => None
    };
}

public record SeriesLink : NoEmptyString
{
    protected SeriesLink(string value) : base(value)
    {
    }

    public new static Option<SeriesLink> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new SeriesLink(value)),
        false => None
    };
}