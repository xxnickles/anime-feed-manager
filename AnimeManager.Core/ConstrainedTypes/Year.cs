using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes
{
    public class Year : Record<Year>
    {
        public readonly Option<ushort> Value;

        public Year(int value)
        {
            if (value >= 2000 && value <= DateTime.Now.Year)
            {
                Value = Some((ushort)value);
            }
            else
            {
                Value = None;
            }
        }

    }
}
