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

public delegate Task<Result<ImmutableList<TvSeriesInfo>>> StoredSeriesGetter(SeriesSeason season,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<AnimeInfoStorage>>> RawStoredSeriesGetter(SeriesSeason season,
    CancellationToken cancellationToken = default);

public static class ExistentSeries
{
    public static StoredSeriesGetter GetExistentStoredSeriesGetter(this ITableClientFactory clientFactory) =>
        (season, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetStoredSeries(season, token))
                .Map(series => series.ConvertAll(Mapper));

    public static RawStoredSeriesGetter GetRawExistentStoredSeriesGetter(this ITableClientFactory clientFactory) =>
        (season, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.GetStoredSeries(season, token));


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
                nameof(AnimeInfoStorage.Title),
                nameof(AnimeInfoStorage.FeedTitle),
                nameof(AnimeInfoStorage.AlternativeTitles),
                nameof(AnimeInfoStorage.Status),
                nameof(AnimeInfoStorage.ImageUrl)
            ],
            cancellationToken: cancellationToken));
    }

    private static TvSeriesInfo Mapper(AnimeInfoStorage entity)
        => string.IsNullOrWhiteSpace(entity.ImageUrl)
            ? new TvSeriesInfo(
                entity.Title ?? string.Empty,
                entity.FeedTitle,
                ConvertAlternativeTitles(entity.AlternativeTitles),
                (SeriesStatus) entity.Status)
            : new TvSeriesInfoWithImage(entity.Title ?? string.Empty,
                entity.FeedTitle,
                ConvertAlternativeTitles(entity.AlternativeTitles),
                (SeriesStatus) entity.Status,
                entity.ImageUrl ?? string.Empty);

    private static string[] ConvertAlternativeTitles(string? alternativeTitles) =>
        alternativeTitles?.Split(SharedUtils.ArraySeparator) ?? [];
}