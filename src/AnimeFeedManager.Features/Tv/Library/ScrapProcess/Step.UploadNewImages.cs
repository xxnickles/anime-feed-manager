namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class UploadNewImages
{
    // private static Task<Result<UpdateLibraryProcessData>> UploadNewImagesToStorage(
    //     Task<Result<UpdateLibraryProcessData>> processResult,
    //     IImagesStore imagesStore,
    //     CancellationToken token)
    // {
    //  
    // }
    //
    //
    // private static Task<Result<UpdateLibraryProcessData>> UploadImages(UpdateLibraryProcessData processData,
    //     IImagesStore imagesStore,
    //     CancellationToken token)
    // {
    //     var t = processData.SeriesData
    //         .Where(series => series.Image is ScrappedImageUrl)
    //         .AsParallel()
    //         .Select(series => series.Image);
    // }
    //
    
    // private Result<Uri> UploadImage(
    //     string name,
    //     Stream image,
    //     SeriesSeason season,
    //     IImagesStore imagesStore, 
    //     CancellationToken token)
    // {
    //     var partition = IdHelpers.GenerateAnimePartitionKey(season);
    //     var directory = $"{season.Year.ToString()}/{season.Season.ToString()}";
    //     return imagesStore.Upload(imageUrl, partition, directory, token);
    // }
}