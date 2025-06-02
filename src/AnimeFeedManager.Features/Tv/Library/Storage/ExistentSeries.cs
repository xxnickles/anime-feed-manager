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

public static class ExistentSeries
{
    public static StoredSeriesGetter GetExistentStoredSeriesGetter(this ITableClientFactory clientFactory) => (season, token) =>
        clientFactory.GetClient<AnimeInfoStorage>(token)
            .Bind(client => client.GetStoredSeries(season, token));

    private static Task<Result<ImmutableList<TvSeriesInfo>>> GetStoredSeries(
        this AppTableClient<AnimeInfoStorage> tableClient, 
        SeriesSeason season,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.ExecuteQuery(client => client.QueryAsync<AnimeInfoStorage>(partitionKey,
                select:
                [
                    nameof(AnimeInfoStorage.Title),
                    nameof(AnimeInfoStorage.FeedTitle),
                    nameof(AnimeInfoStorage.AlternativeTitles),
                    nameof(AnimeInfoStorage.Status),
                    nameof(AnimeInfoStorage.ImageUrl)
                ],
                cancellationToken: cancellationToken))
            .Map(series => series.ConvertAll(Mapper));
    }

    private static TvSeriesInfo Mapper(AnimeInfoStorage entity)
        => !string.IsNullOrWhiteSpace(entity.ImageUrl)
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

    private static string[] ConvertAlternativeTitles(string? alternativeTitles) => alternativeTitles?.Split(SharedUtils.ArraySeparator) ?? [];
}