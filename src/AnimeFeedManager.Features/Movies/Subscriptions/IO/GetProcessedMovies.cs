using System.Text.Json;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Subscriptions.IO;

public readonly record struct FeedProcessedMovie(string SeriesTitle, SeriesFeedLinks[] Links);

public interface IGetProcessedMovies
{
    Task<Either<DomainError, ImmutableList<FeedProcessedMovie>>> GetForSeason(PartitionKey partitionKey,
        CancellationToken token);
}

public sealed class GetProcessedMovies : IGetProcessedMovies
{
    private readonly ITableClientFactory<MovieStorage> _tableClientFactory;

    public GetProcessedMovies(ITableClientFactory<MovieStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<FeedProcessedMovie>>> GetForSeason(PartitionKey partitionKey,
        CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<MovieStorage>(
                    storage => storage.PartitionKey == partitionKey && storage.Status == ShortSeriesStatus.Processed,
                    cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(Map));
    }

    private static FeedProcessedMovie Map(MovieStorage movieStorage)
    {
        return new FeedProcessedMovie(movieStorage.Title ?? string.Empty,
            movieStorage.FeedInfo is not null
                ? JsonSerializer.Deserialize(movieStorage.FeedInfo,
                    SeriesFeedLinksContext.Default.SeriesFeedLinksArray) ?? []
                : []);
    }
}