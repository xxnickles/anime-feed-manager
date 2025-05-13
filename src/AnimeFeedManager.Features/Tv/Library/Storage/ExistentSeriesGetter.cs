namespace AnimeFeedManager.Features.Tv.Library.Storage;

public record TvSeriesInfo(string Title, string? FeedTitle, string[] AlternativeTitles, SeriesStatus Status);

public delegate Task<Result<ImmutableList<TvSeriesInfo>>> StoredSeriesGetter(SeriesSeason season,
    CancellationToken cancellationToken = default);

public static class ExistentSeriesGetter
{
    public static StoredSeriesGetter GetStoredSeries(ITableClientFactory clientFactory) => (season, token) =>
        clientFactory.GetClient<AnimeInfoStorage>(token)
            .Bind(client => client.GetStoredSeries(season, token));

    private static Task<Result<ImmutableList<TvSeriesInfo>>> GetStoredSeries(
        this AppTableClient<AnimeInfoStorage> tableClient, SeriesSeason season,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season.Season, season.Year);
        return tableClient.ExecuteQuery(client => client.QueryAsync<AnimeInfoStorage>(partitionKey,
                select:
                [
                    nameof(AnimeInfoStorage.Title),
                    nameof(AnimeInfoStorage.FeedTitle),
                    nameof(AnimeInfoStorage.AlternativeTitles),
                    nameof(AnimeInfoStorage.Status)
                ],
                cancellationToken: cancellationToken))
            .Map(series => series.ConvertAll(s => new TvSeriesInfo(
                s.Title ?? string.Empty,
                s.FeedTitle,
                ConvertAlternativeTitles(s.AlternativeTitles),
                (SeriesStatus) (s.Status ?? string.Empty))));
    }

    private static string[] ConvertAlternativeTitles(string? alternativeTitles) => alternativeTitles?.Split('|') ?? [];
}