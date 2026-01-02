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
                    .WithOperationName("TableStorageTvLibraryGetter")
                    .WithLogProperties([
                        new KeyValuePair<string, object>("Season", season),
                        new KeyValuePair<string, object>("PublicBlobUri", blobUri)
                    ])
                    .Bind(client => client
                        .ExecuteQuery<AnimeInfoStorage>(
                            series => series.PartitionKey ==
                                      IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year), token)
                        .Map(series => series.ConvertAll(s => LibraryMapper(s, blobUri))));

        public TvLibrarySeries TableStorageTvLibrarySeries =>
            (season, id, blobUri, token) => clientFactory.GetClient<AnimeInfoStorage>()
                .WithOperationName("TableStorageTvLibrarySeries")
                .WithLogProperties([
                    new KeyValuePair<string, object>("Season", season),
                    new KeyValuePair<string, object>("BlobUri", blobUri),
                ])
                .Bind(client => client
                    .ExecuteQuery<AnimeInfoStorage>(
                        series =>
                            series.PartitionKey == IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year) &&
                            series.RowKey == id, token)
                    .SingleItemOrNotFound()
                    .Map(s => LibraryMapper(s, blobUri)));

        public TvSeriesGetter TableStorageTvSeriesGetter =>
            (id, season, token) => clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetAnimeInfo(id, season, token));

        public OnGoingStoredTvSeries TableStorageOnGoingStoredTvSeries =>
            token => clientFactory.GetClient<AnimeInfoStorage>()
                .WithOperationName("TableStorageOnGoingStoredTvSeries")
                .Bind(client =>
                    client.ExecuteQuery<AnimeInfoStorage>(series => series.Status == SeriesStatus.Ongoing(), token));
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
            .WithOperationName("GetStoredSeries")
            .WithLogProperty("Season", season);
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
            .WithOperationName(nameof(GetAnimeInfo))
            .WithLogProperties([
                new KeyValuePair<string, object>("Id", id),
                new KeyValuePair<string, object>("Season", seasonString)
            ])
            .Map(clientResult => clientResult.Value);
    }
}