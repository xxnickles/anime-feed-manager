using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Old.Features.Tv.Subscriptions.IO;

public interface IAddProcessedTitles
{
    Task<Either<DomainError, Unit>> Add(IEnumerable<(string User, string Title)> data,
        CancellationToken token);
}

public class AddProcessedTitles(ITableClientFactory<ProcessedTitlesStorage> clientFactory) : IAddProcessedTitles
{
    public Task<Either<DomainError, Unit>> Add(IEnumerable<(string User, string Title)> data,
        CancellationToken token)
    {
        return clientFactory.GetClient()
            .BindAsync(
                client => TableUtils.BatchAdd(
                    client,
                    data.Select(item => new ProcessedTitlesStorage {PartitionKey = item.User, RowKey = item.Title}),
                    token));
    }
}