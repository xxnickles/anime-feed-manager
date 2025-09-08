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

public delegate Task<Result<ImmutableList<TvSeriesInfo>>> TableStorageStoredSeries(SeriesSeason season,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<AnimeInfoStorage>>> TableStorageRawStoredSeries(SeriesSeason season,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<TvSeries>>> TableStorageTvLibrary(
    SeriesSeason season,
    Uri publicBlobUri,
    CancellationToken cancellationToken = default);

public static class ExistentSeries
{
    public static TableStorageStoredSeries ExistentStoredSeriesGetter(this ITableClientFactory clientFactory) =>
        (season, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetStoredSeries(season, token))
                .Map(series => series.ConvertAll(Mapper));

    public static TableStorageRawStoredSeries RawExistentStoredSeriesGetter(this ITableClientFactory clientFactory) =>
        (season, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetStoredSeries(season, token));


    public static TableStorageTvLibrary TvLibraryGetter(this ITableClientFactory clientFactory) =>
        (season, blobUriBuilder, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetTvLibrary(season, blobUriBuilder, token));

    private static Task<Result<ImmutableList<AnimeInfoStorage>>> GetStoredSeries(
        this AppTableClient tableClient,
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
            cancellationToken: cancellationToken));
    }

    private static Task<Result<ImmutableList<TvSeries>>> GetTvLibrary(
        this AppTableClient tableClient,
        SeriesSeason season,
        Uri publicBlobUriBuilder,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.ExecuteQuery(client => client.QueryAsync<AnimeInfoStorage>(
                series => series.PartitionKey == partitionKey,
                cancellationToken: cancellationToken))
            .Map(series => series.ConvertAll(s => LibraryMapper(s, publicBlobUriBuilder)));
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
        alternativeTitles?.Split(SharedUtils.ArraySeparator) ?? [];
}