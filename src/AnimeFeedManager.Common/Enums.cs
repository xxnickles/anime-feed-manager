using System.Diagnostics;

namespace AnimeFeedManager.Common;

public enum SeriesType
{
    Tv = 0,
    Ova = 1,
    Movie = 2,
    None= 3
}

public static class SeriesTypeExtensions
{
    public static string AsPluralText(this SeriesType shortSeries)
    {
        return shortSeries switch
        {
            SeriesType.Tv => "TV",
            SeriesType.Movie => "Movies",
            SeriesType.Ova => "OVAs",
            _ => throw new UnreachableException($"{nameof(SeriesType)} value is out of range")
        };
    }
}