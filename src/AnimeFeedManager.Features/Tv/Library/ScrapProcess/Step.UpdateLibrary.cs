namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class UpdateTvLibraryStep
{
    internal static Task<Result<ScrapTvLibraryData>> UpdateTvLibrary(
        this Task<Result<ScrapTvLibraryData>> processResult,
        TvLibraryUpdater libraryUpdater,
        CancellationToken token) => processResult
        .Bind(process => libraryUpdater(process.SeriesData.Select(s => s.Series), token).Map(_ => process));
}