using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.Storage;

public record TvSeriesInfo(
    string Title,
    string? FeedTitle,
    string[] AlternativeTitles,
    SeriesStatus Status);

public sealed record TvSeriesInfoWithImage(
    string Title,
    string? FeedTitle,
    string[] AlternativeTitles,
    SeriesStatus Status,
    string ImageUrl) : TvSeriesInfo(Title, FeedTitle, AlternativeTitles, Status);

public sealed record TvSeries(
    string Id,
    string SeasonString,
    string Title,
    string Synopsis,
    string? FeedTitle,
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
    public static StoredSeries TableStorageExistentStoredSeries(this ITableClientFactory clientFactory) =>
        (season, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetStoredSeries(season, token))
                .Map(series => series.ConvertAll(Mapper));

    public static RawStoredSeries TableStorageRawExistentStoredSeries(this ITableClientFactory clientFactory) =>
        (season, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetStoredSeries(season, token));


    public static TvLibrary TableStorageTvLibraryGetter(this ITableClientFactory clientFactory) =>
        (season, blobUriBuilder, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetTvLibrary(season, blobUriBuilder, token));


    public static TvLibrarySeries TableStorageTvLibrarySeries(this ITableClientFactory clientFactory) =>
        (season, id, blobUriBuilder, token) => clientFactory.GetClient<AnimeInfoStorage>()
            .Bind(client => client.GetTvLibrarySeries(season, id, blobUriBuilder, token));

    public static TvSeriesGetter TableStorageTvSeriesGetter(this ITableClientFactory clientFactory) =>
        (id, season, token) => clientFactory.GetClient<AnimeInfoStorage>()
            .Bind(client => client.GetAnimeInfo(id, season, token));

    public static OnGoingStoredTvSeries TableStorageOnGoingStoredTvSeries(this ITableClientFactory clientFactory) =>
        token => clientFactory.GetClient<AnimeInfoStorage>()
            .Bind(client => client.GetOnGoingSeries(token));

    private static Task<Result<ImmutableList<AnimeInfoStorage>>> GetStoredSeries(
        this TableClient tableClient,
        SeriesSeason season,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.ExecuteQuery(client => client.QueryAsync<AnimeInfoStorage>(
                series => series.PartitionKey == partitionKey,
                select:
                [
                    nameof(AnimeInfoStorage.RowKey),
                    nameof(AnimeInfoStorage.PartitionKey),
                    nameof(AnimeInfoStorage.Title),
                    nameof(AnimeInfoStorage.FeedTitle),
                    nameof(AnimeInfoStorage.AlternativeTitles),
                    nameof(AnimeInfoStorage.Status),
                    nameof(AnimeInfoStorage.ImagePath)
                ],
                cancellationToken: cancellationToken))
            .MapError(error => error
                .WithLogProperty("Season", season)
                .WithOperationName(nameof(GetStoredSeries)));
    }

    private static Task<Result<ImmutableList<TvSeries>>> GetTvLibrary(
        this TableClient tableClient,
        SeriesSeason season,
        Uri publicBlobUri,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.ExecuteQuery(client => client.QueryAsync<AnimeInfoStorage>(
                series => series.PartitionKey == partitionKey,
                cancellationToken: cancellationToken))
            .Map(series => series.ConvertAll(s => LibraryMapper(s, publicBlobUri)))
            .MapError(error => error
                .WithLogProperty("Season", season)
                .WithLogProperty("PublicBlobUri", publicBlobUri)
                .WithOperationName(nameof(GetTvLibrary)));
    }

    private static Task<Result<TvSeries>> GetTvLibrarySeries(
        this TableClient tableClient,
        SeriesSeason season,
        string id,
        Uri publicBlobUri,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.ExecuteQuery(client => client.QueryAsync<AnimeInfoStorage>(
                series => series.PartitionKey == partitionKey && series.RowKey == id,
                cancellationToken: cancellationToken)).SingleItemOrNotFound()
            .Map(s => LibraryMapper(s, publicBlobUri))
            .MapError(error => error
                .WithLogProperty("Season", season)
                .WithLogProperty("Id", id)
                .WithLogProperty("publicBlobUri", publicBlobUri)
                .WithOperationName(nameof(TvSeries)));
    }


    private static TvSeries LibraryMapper(AnimeInfoStorage entity, Uri publicBlobUri) => new(
        entity.RowKey ?? string.Empty,
        entity.PartitionKey ?? string.Empty,
        entity.Title ?? string.Empty,
        entity.Synopsis ?? string.Empty,
        entity.FeedTitle,
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
                ConvertAlternativeTitles(entity.AlternativeTitles),
                (SeriesStatus) entity.Status)
            : new TvSeriesInfoWithImage(entity.Title ?? string.Empty,
                entity.FeedTitle,
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

    private static Task<Result<ImmutableList<AnimeInfoStorage>>> GetOnGoingSeries(
        this TableClient tableClient,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client => client.QueryAsync<AnimeInfoStorage>(
                series => series.Status == SeriesStatus.Ongoing(),
                cancellationToken: cancellationToken))
            .MapError(error =>
                error
                    .WithOperationName(nameof(GetOnGoingSeries)));
    }
}