using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Images;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public interface ITvImagesCollector
{
    Task<Result<ScrapTvLibraryData>> AddImagesLink(ScrapTvLibraryData data, CancellationToken token = default);
}

internal sealed class TvImagesCollector : ITvImagesCollector
{
    private readonly IImagesStore _imagesStore;
    private readonly ILogger<TvImagesCollector> _logger;

    public TvImagesCollector(
        IImagesStore imagesStore,
        ILogger<TvImagesCollector> logger)
    {
        _imagesStore = imagesStore;
        _logger = logger;
    }

    public async Task<Result<ScrapTvLibraryData>> AddImagesLink(ScrapTvLibraryData data, CancellationToken token = default)
    {
        var targetDirectory =  $"{data.Season.Year}/{data.Season.Season}";
        var updatedSeriesTasks = data.SeriesData
            .Select(s => AddImageLink(s, targetDirectory, token));

        // Wait for all tasks to complete
        var results = await Task.WhenAll(updatedSeriesTasks);

        return Result<ScrapTvLibraryData>.Success(data with {SeriesData = results});
    }

    private Task<StorageData> AddImageLink(StorageData storageData, string targetDirectory,
        CancellationToken cancellationToken)
    {
        if (storageData is {Image: ScrappedImageUrl scrappedImageUrl, Series.RowKey: not null})
        {
            return _imagesStore.Process(new ImageProcessData(
                    IdHelpers.CleanAndFormatAnimeTitle(storageData.Series.RowKey),
                    targetDirectory,
                    scrappedImageUrl.Url), cancellationToken)
                .MatchToValue(
                    uri => AddUrl(storageData, uri),
                    error => ProcessError(error, storageData, _logger));
        }

        return Task.FromResult(storageData);
    }


    private static StorageData AddUrl(StorageData storageData, Uri imageUrl)
    {
        storageData.Series.ImageUrl = imageUrl.ToString();
        return storageData;
    }

    private static StorageData ProcessError(DomainError error, StorageData storageData, ILogger logger)
    {
        logger.LogError("Couldn't process image for {AnimeTitle}", storageData.Series.RowKey);
        if (error is not HandledError)
            error.LogError(logger);
        return storageData;
    }
}