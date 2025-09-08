using AnimeFeedManager.Features.Images;

namespace AnimeFeedManager.Features.Tv.Library.Queries;

public static class LibraryQueries
{
    public static Task<Result<ImmutableList<TvSeries>>> GetTvLibrary(
        TableStorageTvLibrary libraryGetter,
        SeriesSeason season,
        Uri publicBlobUri,
        CancellationToken cancellationToken) => libraryGetter(season, publicBlobUri, cancellationToken);
}