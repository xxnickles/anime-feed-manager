using System;
using System.Collections.Generic;
using System.Text;
using LanguageExt;

namespace AnimeFeedManager.Core.Utils
{
    public class OptionUtils
    {
        public static T UnpackOption<T>(Option<T> value, T noneValue)
        {
            return value.Match(
                v => v,
                () => noneValue);
        }
    }
}
