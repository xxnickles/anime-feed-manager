using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IGetProcessedTitles
{
    Task<Either<DomainError, ImmutableList<string>>> GetForUser(UserId userId, CancellationToken token);
}

public class GetProcessedTitles(ITableClientFactory<ProcessedTitlesStorage> clientFactory) : IGetProcessedTitles
{
    public Task<Either<DomainError, ImmutableList<string>>> GetForUser(UserId userId, CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<ProcessedTitlesStorage>(item => item.PartitionKey == userId,
                    cancellationToken: token)))
            .MapAsync(storageList => storageList.ConvertAll(ExtractTitle));
    }

    private static string ExtractTitle(ProcessedTitlesStorage storage) =>
        storage.RowKey ?? string.Empty;
}