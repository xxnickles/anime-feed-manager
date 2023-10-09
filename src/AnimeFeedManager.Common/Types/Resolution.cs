namespace AnimeFeedManager.Common.Types;

public readonly struct Resolution
{
    public readonly string Value;

    private Resolution(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static Resolution Sd = new("sd");
    public static Resolution Hd = new("720");
    public static Resolution FullHd = new("1080");
}