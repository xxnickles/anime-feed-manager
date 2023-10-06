using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IAddProcessedTitles
{
    Task<Either<DomainError, Unit>> Add(IEnumerable<(string User, string Title)> data,
        CancellationToken token);
}

public class AddProcessedTitles : IAddProcessedTitles
{
    private readonly ITableClientFactory<ProcessedTitlesStorage> _clientFactory;

    public AddProcessedTitles(ITableClientFactory<ProcessedTitlesStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> Add(IEnumerable<(string User, string Title)> data,
        CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(
                client => TableUtils.BatchAdd(
                    client,
                    data.Select(item => new ProcessedTitlesStorage {PartitionKey = item.User, RowKey = item.Title}),
                    token));
    }
}