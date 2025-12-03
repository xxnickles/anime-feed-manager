using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.Storage.Stores;

public record TvSeriesInfo(
    string Title,
    string? FeedTitle,
    string? FeedUrl,
    string[] AlternativeTitles,
    SeriesStatus Status);

public sealed record TvSeriesInfoWithImage(
    string Title,
    string? FeedTitle,
    string? FeedUrl,
    string[] AlternativeTitles,
    SeriesStatus Status,
    string ImageUrl) : TvSeriesInfo(Title, FeedTitle, FeedUrl, AlternativeTitles, Status);

public sealed record TvSeries(
    string Id,
    string SeasonString,
    string Title,
    string Synopsis,
    string? FeedTitle,
    string? FeedUrl,
    string[] AlternativeTitles,
    SeriesStatus Status,
    Uri? Image);

public delegate Task<Result<ImmutableList<TvSeriesInfo>>> StoredSeries(SeriesSeason season,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<AnimeInfoStorage>>> RawStoredSeries(SeriesSeason season,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<AnimeInfoStorage>>> OnGoingStoredTvSeries(
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<TvSeries>>> TvLibrary(
    SeriesSeason season,
    Uri publicBlobUri,
    CancellationToken cancellationToken = default);

public delegate Task<Result<TvSeries>> TvLibrarySeries(
    SeriesSeason season,
    string id,
    Uri publicBlobUri,
    CancellationToken cancellationToken = default);

public delegate Task<Result<AnimeInfoStorage>> TvSeriesGetter(string id, string season,
    CancellationToken cancellationToken = default);

public static class ExistentSeries
{
    extension(ITableClientFactory clientFactory)
    {
        public StoredSeries TableStorageExistentStoredSeries =>
            (season, token) =>
                clientFactory.GetClient<AnimeInfoStorage>()
                    .Bind(client => client.GetStoredSeries(season, token))
                    .Map(series => series.ConvertAll(Mapper));

        public RawStoredSeries TableStorageRawExistentStoredSeries() =>
            (season, token) =>
                clientFactory.GetClient<AnimeInfoStorage>()
                    .Bind(client => client.GetStoredSeries(season, token));

        public TvLibrary TableStorageTvLibraryGetter =>
            (season, blobUri, token) =>
                clientFactory.GetClient<AnimeInfoStorage>()
                    .Bind(client => client
                        .ExecuteQuery<AnimeInfoStorage>(
                            series => series.PartitionKey ==
                                      IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year), token)
                        .Map(series => series.ConvertAll(s => LibraryMapper(s, blobUri))))
                    .MapError(error => error
                        .WithLogProperty("Season", season)
                        .WithLogProperty("PublicBlobUri", blobUri)
                        .WithOperationName("TableStorageTvLibrary"));

        public TvLibrarySeries TableStorageTvLibrarySeries =>
            (season, id, blobUri, token) => clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client
                    .ExecuteQuery<AnimeInfoStorage>(
                        series => series.PartitionKey == IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year) && series.RowKey == id, token)
                    .SingleItemOrNotFound()
                    .Map(s => LibraryMapper(s, blobUri)))
                .MapError(error => error
                    .WithLogProperty("Season", season)
                    .WithLogProperty("Id", id)
                    .WithLogProperty("BlobUri", blobUri)
                    .WithOperationName("TableStorageTvLibrarySeries"));

        public TvSeriesGetter TableStorageTvSeriesGetter =>
            (id, season, token) => clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetAnimeInfo(id, season, token));

        public OnGoingStoredTvSeries TableStorageOnGoingStoredTvSeries =>
            token => clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client =>
                    client.ExecuteQuery<AnimeInfoStorage>(series => series.Status == SeriesStatus.Ongoing(), token))
                .MapError(error =>
                    error
                        .WithOperationName("TableStorageOnGoingStoredTvSeries"));
    }


    private static Task<Result<ImmutableList<AnimeInfoStorage>>> GetStoredSeries(
        this TableClient tableClient,
        SeriesSeason season,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.ExecuteQuery<AnimeInfoStorage>(
                series => series.PartitionKey == partitionKey,
                cancellationToken,
                [
                    nameof(AnimeInfoStorage.RowKey),
                    nameof(AnimeInfoStorage.PartitionKey),
                    nameof(AnimeInfoStorage.Title),
                    nameof(AnimeInfoStorage.FeedTitle),
                    nameof(AnimeInfoStorage.AlternativeTitles),
                    nameof(AnimeInfoStorage.Status),
                    nameof(AnimeInfoStorage.ImagePath)
                ])
            .MapError(error => error
                .WithLogProperty("Season", season)
                .WithOperationName(nameof(GetStoredSeries)));
    }


    private static TvSeries LibraryMapper(AnimeInfoStorage entity, Uri publicBlobUri) => new(
        entity.RowKey ?? string.Empty,
        entity.PartitionKey ?? string.Empty,
        entity.Title ?? string.Empty,
        entity.Synopsis ?? string.Empty,
        entity.FeedTitle,
        entity.FeedLink,
        ConvertAlternativeTitles(entity.AlternativeTitles),
        (SeriesStatus) entity.Status,
        entity.ImagePath is not null ? GetUri(publicBlobUri, entity.ImagePath) : null);

    private static Uri GetUri(Uri publicBlobUri, string imagePath)
    {
        var baseAsDir = publicBlobUri.AbsoluteUri.EndsWith("/")
            ? publicBlobUri
            : new Uri(publicBlobUri.AbsoluteUri + "/");

        return new Uri(baseAsDir, imagePath);
    }

    private static TvSeriesInfo Mapper(AnimeInfoStorage entity)
        => string.IsNullOrWhiteSpace(entity.ImagePath)
            ? new TvSeriesInfo(
                entity.Title ?? string.Empty,
                entity.FeedTitle,
                entity.FeedLink,
                ConvertAlternativeTitles(entity.AlternativeTitles),
                (SeriesStatus) entity.Status)
            : new TvSeriesInfoWithImage(entity.Title ?? string.Empty,
                entity.FeedTitle,
                entity.FeedLink,
                ConvertAlternativeTitles(entity.AlternativeTitles),
                (SeriesStatus) entity.Status,
                entity.ImagePath ?? string.Empty);

    private static string[] ConvertAlternativeTitles(string? alternativeTitles) =>
        string.IsNullOrWhiteSpace(alternativeTitles) ? [] : alternativeTitles.Split(SharedUtils.ArraySeparator);

    private static Task<Result<AnimeInfoStorage>> GetAnimeInfo(
        this TableClient tableClient,
        string id,
        string seasonString,
        CancellationToken cancellationToken = default)
    {
        return tableClient.TryExecute<AnimeInfoStorage>(client =>
                client.GetEntityAsync<AnimeInfoStorage>(seasonString, id, cancellationToken: cancellationToken))
            .Map(clientResult => clientResult.Value)
            .MapError(error => error
                .WithOperationName(nameof(GetAnimeInfo))
                .WithLogProperty("Id", id)
                .WithLogProperty("Season", seasonString));
    }
}