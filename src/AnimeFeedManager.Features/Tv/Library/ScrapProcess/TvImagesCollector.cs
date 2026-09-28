using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Images;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class TvImagesCollector
{
    // The provider answers a burst with a 429 that bans the whole caller IP range for hours, so
    // downloads go out in small batches with a pause between them — an import that takes a minute
    // longer costs far less than one that poisons the next several. Sized by judgement, not
    // measurement: the provider publishes no limits.
    private const int BatchSize = 4;
    private static readonly TimeSpan BatchPause = TimeSpan.FromMilliseconds(500);

    private static readonly ActivitySource Source = new(Telemetry.ImagesSource);

    public static async Task<Result<ScrapTvLibraryData>> AddImagesLink(
        this ImageProcessor imageProvider,
        ScrapTvLibraryData data,
        CancellationToken token = default)
    {
        var targetDirectory = $"{data.Season.Year}/{data.Season.Season}";
        // SeriesData is a deferred projection and is walked more than once here.
        var seriesData = data.SeriesData.ToArray();

        // Only series that will actually issue a request are paced; everything else passes straight
        // through, so a re-import that needs no covers still costs nothing.
        var pending = seriesData.Select(ToPendingImage).OfType<PendingImage>().ToArray();
        var untouched = seriesData
            .Where(series => ToPendingImage(series) is null)
            .Select(series => Result<ImageOutcome>.Success(new ImageOutcome(series, false)));

        var downloaded = new List<Result<ImageOutcome>>(pending.Length);
        foreach (var batch in pending.Chunk(BatchSize))
        {
            // Skipped ahead of the first batch, so a single-batch import never waits.
            if (downloaded.Count > 0)
                await Task.Delay(BatchPause, token);

            downloaded.AddRange(await Task.WhenAll(
                batch.Select(image => AddImageLink(imageProvider, image, targetDirectory, token))));
        }

        return downloaded.Concat(untouched)
            .Flatten(items => items.ToImmutableArray())
            .AddLogOnSuccess(bulk => LogImageOutcome(bulk, pending.Length, seriesData.Length))
            .AddLogOnSuccess(bulk => bulk.LogErrors)
            .Map(bulk => data with { SeriesData = bulk.Value.Select(outcome => outcome.Data) });
    }

    private readonly record struct ImageOutcome(StorageData Data, bool Downloaded);

    /// <summary>A series carrying everything a download needs, resolved once at the match point.</summary>
    private readonly record struct PendingImage(StorageData Data, string RowKey, Uri Url);

    private static PendingImage? ToPendingImage(StorageData series) =>
        series is {Image: ScrappedImageUrl scrapped, Series.RowKey: { } rowKey}
            ? new PendingImage(series, rowKey, scrapped.Url)
            : null;

    // Series with nothing to fetch pass through this step as successes, so the only honest count is
    // of the ones that actually had an image to download.
    private static Action<ILogger> LogImageOutcome(
        BulkResult<ImmutableArray<ImageOutcome>> bulk,
        int candidates,
        int total) => logger =>
    {
        var downloaded = bulk.Value.Count(outcome => outcome.Downloaded);

        logger.LogInformation(
            "{Downloaded} of {Candidates} images downloaded, {Failed} series kept without one; {Skipped} of {Total} series had no image to fetch",
            downloaded, candidates, candidates - downloaded, total - candidates, total);
    };

    private static async Task<Result<ImageOutcome>> AddImageLink(
        ImageProcessor imageProvider,
        PendingImage pending,
        string targetDirectory,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Images");
        return await imageProvider(new ImageProcessData(
                IdHelpers.CleanAndFormatAnimeTitle(pending.RowKey),
                targetDirectory,
                pending.Url), cancellationToken)
            .MarkActivityErroredOnError()
            .Map(uri => new ImageOutcome(AddUrl(pending.Data, uri), true))
            // A cover that will not download must not cost the series its row in the library.
            .AddLogOnFailure(error => logger => logger.LogWarning(
                "Storing {Series} without an image: {Reason}", pending.Data.Series.Title, error.Message))
            .BindOnError(_ => new ImageOutcome(pending.Data, false));
    }


    private static StorageData AddUrl(StorageData storageData, Uri imageUrl)
    {
        var series = storageData.Series;
        series.ImagePath = imageUrl.ToString();
        return storageData with { Series = series };
    }
}