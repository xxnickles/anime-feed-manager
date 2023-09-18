using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IAddProcessedTitle
{
    Task<Either<DomainError, Unit>> AddForUser(UserId user, NoEmptyString title, CancellationToken token);
}

public class AddProcessedTitle : IAddProcessedTitle
{
    private readonly ITableClientFactory<ProcessedTitlesStorage> _clientFactory;

    public AddProcessedTitle(ITableClientFactory<ProcessedTitlesStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> AddForUser(UserId user, NoEmptyString title, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(
                client => TableUtils.TryExecute(() =>
                    client.UpsertEntityAsync(new ProcessedTitlesStorage { PartitionKey = user, RowKey = title },
                        cancellationToken: token)))
            .MapAsync(_ => unit);
    }
}