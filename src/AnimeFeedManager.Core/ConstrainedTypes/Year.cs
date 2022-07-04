using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes;

public class Year : Record<Year>
{
    public readonly Option<ushort> Value;

    private Year(int value)
    {
        if (value >= 2000 && value <= DateTime.Now.Year + 1)
        {
            Value = Some((ushort)value);
        }
        else
        {
            Value = None;
        }
    }

    public static Year FromNumber(int value) => new (value);

}