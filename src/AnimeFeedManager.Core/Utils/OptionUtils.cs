using LanguageExt;

namespace AnimeFeedManager.Core.Utils;

public static class OptionUtils
{
    public static T UnpackOption<T>(this Option<T> value, T noneValue)
    {
        return value.Match(
            v => v,
            () => noneValue);
    }
}