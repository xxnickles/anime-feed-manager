namespace AnimeFeedManager.Shared.Types;

public enum SeriesType
{
    None,
    Tv,
    Ova,
    Movie
}

public static class Extensions
{
    public static string AsPlural(this SeriesType shortSeries)
    {
        return shortSeries switch
        {
            SeriesType.Tv => "tv",
            SeriesType.Movie => "movies",
            SeriesType.Ova => "ovas",
            SeriesType.None => "none",
            _ => throw new UnreachableException($"{nameof(SeriesType)} value is out of range")
        };
    }
}