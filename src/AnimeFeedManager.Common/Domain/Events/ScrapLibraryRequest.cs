namespace AnimeFeedManager.Common.Domain.Events
{
    public enum ScrapType
    {
        Latest,
        BySeason
    }

    public record SeasonParameter(string Season, ushort Year);

    public record ScrapLibraryRequest(SeriesType Type, SeasonParameter? SeasonParameter, ScrapType ScrapType);

    public static class Extensions
    {
        public static SeasonParameter ToSeasonParameter(this (Season season, Year year) param) =>
            new(param.season, param.year);
    }
}

