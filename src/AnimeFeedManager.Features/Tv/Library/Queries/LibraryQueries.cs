namespace AnimeFeedManager.Features.Tv.Library.Queries;

public static class LibraryQueries
{
    public static Task<Result<ImmutableList<TvSeries>>> GetTvLibrary(
        TableStorageTvLibrary libraryGetter,
        SeriesSeason season, 
        CancellationToken cancellationToken) => libraryGetter(season, cancellationToken);
}