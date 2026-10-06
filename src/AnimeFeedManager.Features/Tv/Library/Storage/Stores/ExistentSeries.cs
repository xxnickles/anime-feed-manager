using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.Storage.Stores;

public record TvSeriesInfo(
    string Title,
    string? FeedTitle,
    string? FeedUrl,
    AlternativeTitlesData AlternativeTitles,
    SeriesStatus Status);

public sealed record TvSeriesInfoWithImage(
    string Title,
    string? FeedTitle,
    string? FeedUrl,
    AlternativeTitlesData AlternativeTitles,
    SeriesStatus Status,
    string ImageUrl) : TvSeriesInfo(Title, FeedTitle, FeedUrl, AlternativeTitles, Status);

public sealed record TvSeries(
    string Id,
    SeriesSeason Season,
    string Title,
    string Synopsis,
    string? FeedTitle,
    string? FeedUrl,
    AlternativeTitlesData AlternativeTitles,
    SeriesStatus Status,
    Uri? Image,
    DateTimeOffset? LastUpdated);

public delegate Task<Result<ImmutableArray<TvSeriesInfo>>> StoredSeriesGetter(SeriesSeason season,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<AnimeInfoStorage>>> RawStoredSeries(SeriesSeason season,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<AnimeInfoStorage>>> OnGoingStoredTvSeries(
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<TvSeries>>> TvLibrary(
    SeriesSeason season,
    Uri publicBlobUri,
    CancellationToken cancellationToken = default);

public delegate Task<Result<TvSeries>> TvLibrarySeries(
    string id,
    SeriesSeason season,
    Uri publicBlobUri,
    CancellationToken cancellationToken = default);

public delegate Task<Result<AnimeInfoStorage>> TvSeriesGetter(string id, SeriesSeason season,
    CancellationToken cancellationToken = default);

public static class ExistentSeries
{
    extension(ITableClientFactory clientFactory)
    {
        public StoredSeriesGetter TableStorageExistentStoredSeriesGetter =>
            (season, token) =>
                clientFactory.GetClient<AnimeInfoStorage>()
                    .Bind(client => client.GetStoredSeries(season, token))
                    .Map(series => series.Select(Mapper).ToImmutableArray());

        public RawStoredSeries TableStorageRawExistentStoredSeries() =>
            (season, token) =>
                clientFactory.GetClient<AnimeInfoStorage>()
                    .Bind(client => client.GetStoredSeries(season, token));

        public TvLibrary TableStorageTvLibraryGetter =>
            (season, blobUri, token) =>
                clientFactory.GetClient<AnimeInfoStorage>()
                    .WithOperationName("TableStorageTvLibraryGetter")
                    .WithLogProperties([
                        new KeyValuePair<string, object>("Season", season),
                        new KeyValuePair<string, object>("PublicBlobUri", blobUri)
                    ])
                    .Bind(client => client
                        .ExecuteQuery<AnimeInfoStorage>(
                            series => series.PartitionKey ==
                                      IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year), token)
                        .Map(series => series.Select(s => LibraryMapper(s, season, blobUri)).ToImmutableArray()));

        public TvLibrarySeries TableStorageTvLibrarySeries =>
            (id, season, blobUri, token) => clientFactory.GetClient<AnimeInfoStorage>()
                .WithOperationName("TableStorageTvLibrarySeries")
                .WithLogProperty("PublicBlobUri", blobUri)
                .Bind(client => client.GetAnimeInfo(id, season, token))
                .Map(series => LibraryMapper(series, season, blobUri));

        public TvSeriesGetter TableStorageTvSeriesGetter =>
            (id, season, token) => clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetAnimeInfo(id, season, token));

        public OnGoingStoredTvSeries TableStorageOnGoingStoredTvSeries =>
            token => clientFactory.GetClient<AnimeInfoStorage>()
                .WithOperationName("TableStorageOnGoingStoredTvSeries")
                .Bind(client =>
                    client.ExecuteQuery<AnimeInfoStorage>(series => series.Status == SeriesStatus.Ongoing(), token));
    }


    private static Task<Result<ImmutableArray<AnimeInfoStorage>>> GetStoredSeries(
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
            .WithOperationName("GetStoredSeries")
            .WithLogProperty("Season", season);
    }


    private static TvSeries LibraryMapper(AnimeInfoStorage entity, SeriesSeason season, Uri publicBlobUri) => new(
        entity.RowKey ?? string.Empty,
        season,
        entity.Title ?? string.Empty,
        entity.Synopsis ?? string.Empty,
        entity.FeedTitle,
        entity.FeedLink,
        StoredAlternativeTitles.Parse(entity.AlternativeTitles),
        (SeriesStatus) entity.Status,
        entity.ImagePath is not null ? GetUri(publicBlobUri, entity.ImagePath) : null,
        entity.Timestamp);

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
                StoredAlternativeTitles.Parse(entity.AlternativeTitles),
                (SeriesStatus) entity.Status)
            : new TvSeriesInfoWithImage(entity.Title ?? string.Empty,
                entity.FeedTitle,
                entity.FeedLink,
                StoredAlternativeTitles.Parse(entity.AlternativeTitles),
                (SeriesStatus) entity.Status,
                entity.ImagePath ?? string.Empty);

    private static Task<Result<AnimeInfoStorage>> GetAnimeInfo(
        this TableClient tableClient,
        string id,
        SeriesSeason season,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.TryExecute<AnimeInfoStorage>(client =>
                client.GetEntityAsync<AnimeInfoStorage>(partitionKey, id, cancellationToken: cancellationToken))
            .WithOperationName(nameof(GetAnimeInfo))
            .WithLogProperties([
                new KeyValuePair<string, object>("Id", id),
                new KeyValuePair<string, object>("Season", partitionKey)
            ])
            .Map(clientResult => clientResult.Value);
    }
}