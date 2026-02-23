using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Images;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class TvImagesCollector
{
    private const int BatchSize = 10;

    public static async Task<Result<ScrapTvLibraryData>> AddImagesLink(
        this ImageProcessor imageProvider,
        ScrapTvLibraryData data,
        CancellationToken token = default)
    {
        var targetDirectory = $"{data.Season.Year}/{data.Season.Season}";
        var all = new List<Result<StorageData>>();

        foreach (var batch in data.SeriesData.Chunk(BatchSize))
        {
            var batchResults = await Task.WhenAll(
                batch.Select(s => AddImageLink(imageProvider, s, targetDirectory, token)));
            all.AddRange(batchResults);
        }

        return all
            .Flatten(items => items.ToImmutableList())
            .AddLogOnSuccess(LogFactories.LogBulkResult<ImmutableList<StorageData>>(
                (items, logger) => logger.LogInformation("{Count} images processed", items.Count)))
            .Map(bulk => data with { SeriesData = bulk.Value });
    }

    private static async Task<Result<StorageData>> AddImageLink(
        ImageProcessor imageProvider,
        StorageData storageData,
        string targetDirectory,
        CancellationToken cancellationToken)
    {
        if (storageData is { Image: ScrappedImageUrl scrappedImageUrl, Series.RowKey: not null })
        {
            return await imageProvider(new ImageProcessData(
                    IdHelpers.CleanAndFormatAnimeTitle(storageData.Series.RowKey),
                    targetDirectory,
                    scrappedImageUrl.Url), cancellationToken)
                .Map(uri => AddUrl(storageData, uri));
        }

        return storageData;
    }


    private static StorageData AddUrl(StorageData storageData, Uri imageUrl)
    {
        var series = storageData.Series;
        series.ImagePath = imageUrl.ToString();
        return storageData with { Series = series };
    }
}