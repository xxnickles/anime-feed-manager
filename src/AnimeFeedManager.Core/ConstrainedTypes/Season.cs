using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes
{
    public struct Season : IComparable<Season>, IEquatable<Season>
    {
        public readonly string Value;
        private const string SpringValue = "spring";
        private const string SummerValue = "summer";
        private const string FallValue = "fall";
        private const string WinterValue = "winter";

        private Season(string value)
        {
            Value = value;
        }

        public int CompareTo(Season other)
        {
            return this > other ? 1 : -1;
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(Season other)
        {
            return other.Value == this.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Season item && ReferenceEquals(item.Value, this.Value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Season Spring = new Season(SpringValue);
        public static Season Summer = new Season(SummerValue);
        public static Season Fall = new Season(FallValue);
        public static Season Winter = new Season(WinterValue);

        public static Season FromString(string? val) => val != null ? val.ToLowerInvariant() switch
        {
            SpringValue => Spring,
            SummerValue => Summer,
            FallValue => Fall,
            WinterValue => Winter,
            _ => Spring
        } : Spring;

        public static Option<Season> TryCreateFromString(string val)
        {

            return val switch
            {
                SpringValue => Some(Spring),
                SummerValue => Some(Summer),
                FallValue => Some(Fall),
                WinterValue => Some(Winter),
                _ => None
            };
        }

        // Operators
        public static bool operator >(Season a, Season b) => GetSeasonWeight(a) > GetSeasonWeight(b);
        public static bool operator <(Season a, Season b) => GetSeasonWeight(a) < GetSeasonWeight(b);

        private static byte GetSeasonWeight(Season season) => season.Value switch
        {
            WinterValue => (byte)0,
            SpringValue => (byte)1,
            SummerValue => (byte)2,
            FallValue => (byte)3,
            _ => (byte)0
        };

    }
}