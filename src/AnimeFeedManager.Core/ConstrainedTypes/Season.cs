using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes
{
    public struct Season
    {
        public readonly string Value;

        private Season(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public static Season Spring = new Season("spring");
        public static Season Summer = new Season("summer");
        public static Season Fall = new Season("fall");
        public static Season Winter = new Season("winter");

        public static Season FromString(string? val) => val != null ? val.ToLowerInvariant() switch
        {
            "spring" => Spring,
            "summer" => Summer,
            "fall" => Fall,
            "winter" => Winter,
            _ => Spring
        } : Spring;

        public static Option<Season> TryCreateFromString(string val)
        {

            return val switch
            {
                "spring" => Some(Spring),
                "summer" => Some(Summer),
                "fall" => Some(Fall),
                "winter" => Some(Winter),
                _ => None
            };
        }

    }
}