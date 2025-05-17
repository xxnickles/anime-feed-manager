namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal abstract record ImageInformation;

internal sealed record NoImage : ImageInformation;

internal sealed record AlreadyExistInSystem : ImageInformation;

internal sealed record ScrappedImageUrl(Uri Url) : ImageInformation;

internal sealed record StorageData(
    AnimeInfoStorage Series,
    ImageInformation Image);

internal sealed record ScrapTvLibraryData(
    string Id,
    IEnumerable<StorageData> SeriesData,
    ImmutableList<string> FeedTitles,
    SeriesSeason Season);

internal delegate Task<Result<ImmutableList<string>>> FeedTitlesProvider();