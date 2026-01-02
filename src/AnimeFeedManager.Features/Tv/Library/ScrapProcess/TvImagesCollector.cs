using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Images;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class TvImagesCollector
{
    private const int BatchSize = 10;

    public static async Task<Result<ScrapTvLibraryData>> AddImagesLink(
        this IImageProvider imageProvider,
        ScrapTvLibraryData data,
        ILogger logger,
        CancellationToken token = default)
    {
        var targetDirectory = $"{data.Season.Year}/{data.Season.Season}";
        var updatedSeriesTasks = data.SeriesData
            .Select(s => AddImageLink(imageProvider, s, targetDirectory, logger, token));

        // Process tasks in batches of 10
        var results = new List<StorageData>();
      
        var batches = updatedSeriesTasks
            .Select((task, index) => new { Task = task, Index = index })
            .GroupBy(x => x.Index / BatchSize)
            .Select(g => g.Select(x => x.Task));

        foreach (var batch in batches)
        {
            var batchResults = await Task.WhenAll(batch);
            results.AddRange(batchResults);
        }

        return Result<ScrapTvLibraryData>.Success(data with { SeriesData = results });
    }

    private static async Task<StorageData> AddImageLink(
        IImageProvider imageProvider,
        StorageData storageData,
        string targetDirectory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (storageData is { Image: ScrappedImageUrl scrappedImageUrl, Series.RowKey: not null })
        {
            return await imageProvider.Process(new ImageProcessData(
                    IdHelpers.CleanAndFormatAnimeTitle(storageData.Series.RowKey),
                    targetDirectory,
                    scrappedImageUrl.Url), cancellationToken)
                .MatchToValue(
                    uri => AddUrl(storageData, uri),
                    error => ProcessError(error, storageData, logger));
        }

        return storageData;
    }


    private static StorageData AddUrl(StorageData storageData, Uri imageUrl)
    {
        var series = storageData.Series;
        series.ImagePath = imageUrl.ToString();
        return storageData with { Series = series };
    }

    private static StorageData ProcessError(DomainError error, StorageData storageData, ILogger logger)
    {
        logger.LogError("Couldn't process image for {AnimeTitle}", storageData.Series.RowKey);
        if (error is not HandledError)
            error.WriteError(logger);
        return storageData;
    }
}