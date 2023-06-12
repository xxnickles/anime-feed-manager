namespace AnimeFeedManager.Features.Common.Types;

public readonly record struct Year 
{
    public readonly Option<ushort> Value;

    private Year(int value)
    {
        Value = IsValid(value) ? Some((ushort)value) : None;
    }

    public static Year FromNumber(int value) => new (value);

    public static bool IsValid(int value) => value >= 2000 && value <= DateTime.Now.Year + 1;

}