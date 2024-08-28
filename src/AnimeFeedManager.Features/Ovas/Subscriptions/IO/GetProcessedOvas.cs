using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Subscriptions.IO;

public readonly record struct FeedProcessedOva(string SeriesTitle, SeriesFeedLinks[] Links);

public interface IGetProcessedOvas
{
    Task<Either<DomainError, ImmutableList<FeedProcessedOva>>> GetForSeason(PartitionKey partitionKey,
        CancellationToken token);
}

public class GetProcessedOvas : IGetProcessedOvas
{
    private readonly ITableClientFactory<OvaStorage> _tableClientFactory;

    public GetProcessedOvas(ITableClientFactory<OvaStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<FeedProcessedOva>>> GetForSeason(PartitionKey partitionKey,
        CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<OvaStorage>(
                    storage => storage.PartitionKey == partitionKey && storage.Status == ShortSeriesStatus.Processed,
                    cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(Map));
    }

    private static FeedProcessedOva Map(OvaStorage ovaStorage)
    {
        return new FeedProcessedOva(ovaStorage.Title ?? string.Empty,
            ovaStorage.FeedInfo is not null
                ? JsonSerializer.Deserialize(ovaStorage.FeedInfo,
                    SeriesFeedLinksContext.Default.SeriesFeedLinksArray) ?? []
                : []);
    }
}