using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Images;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class TvImagesCollector
{
    private const int BatchSize = 10;

    private static readonly ActivitySource Source = new(Telemetry.ImagesSource);

    public static async Task<Result<ScrapTvLibraryData>> AddImagesLink(
        this ImageProcessor imageProvider,
        ScrapTvLibraryData data,
        CancellationToken token = default)
    {
        var targetDirectory = $"{data.Season.Year}/{data.Season.Season}";
        // SeriesData is a deferred projection and is walked more than once here.
        var seriesData = data.SeriesData.ToArray();
        var candidates = seriesData.Count(series => series.Image is ScrappedImageUrl);
        var all = new List<Result<ImageOutcome>>();

        foreach (var batch in seriesData.Chunk(BatchSize))
        {
            var batchResults = await Task.WhenAll(
                batch.Select(s => AddImageLink(imageProvider, s, targetDirectory, token)));
            all.AddRange(batchResults);
        }

        return all
            .Flatten(items => items.ToImmutableArray())
            .AddLogOnSuccess(bulk => LogImageOutcome(bulk, candidates, seriesData.Length))
            .AddLogOnSuccess(bulk => bulk.LogErrors)
            .Map(bulk => data with { SeriesData = bulk.Value.Select(outcome => outcome.Data) });
    }

    private readonly record struct ImageOutcome(StorageData Data, bool Downloaded);

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
        StorageData storageData,
        string targetDirectory,
        CancellationToken cancellationToken)
    {
        if (storageData is not { Image: ScrappedImageUrl scrappedImageUrl, Series.RowKey: not null })
            return new ImageOutcome(storageData, false);

        using var activity = Source.StartActivity("Images");
        return await imageProvider(new ImageProcessData(
                IdHelpers.CleanAndFormatAnimeTitle(storageData.Series.RowKey),
                targetDirectory,
                scrappedImageUrl.Url), cancellationToken)
            .MarkActivityErroredOnError()
            .Map(uri => new ImageOutcome(AddUrl(storageData, uri), true))
            // A cover that will not download must not cost the series its row in the library.
            .AddLogOnFailure(error => logger => logger.LogWarning(
                "Storing {Series} without an image: {Reason}", storageData.Series.Title, error.Message))
            .BindOnError(_ => new ImageOutcome(storageData, false));
    }


    private static StorageData AddUrl(StorageData storageData, Uri imageUrl)
    {
        var series = storageData.Series;
        series.ImagePath = imageUrl.ToString();
        return storageData with { Series = series };
    }
}