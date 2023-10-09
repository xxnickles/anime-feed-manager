using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO
{
    public interface IGetProcessedTitles
    {
        Task<Either<DomainError, ImmutableList<string>>> GetForUser(UserId userId, CancellationToken token);
    }

    public class GetProcessedTitles : IGetProcessedTitles
    {
        private readonly ITableClientFactory<ProcessedTitlesStorage> _clientFactory;

        public GetProcessedTitles(ITableClientFactory<ProcessedTitlesStorage> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public Task<Either<DomainError, ImmutableList<string>>> GetForUser(UserId userId, CancellationToken token)
        {
            return _clientFactory.GetClient()
                .BindAsync(client => TableUtils.ExecuteQueryWithEmpty(() =>
                    client.QueryAsync<ProcessedTitlesStorage>(item => item.PartitionKey == userId,
                        cancellationToken: token)))
                .MapAsync(storageList => storageList.ConvertAll(ExtractTitle));
        }

        private static string ExtractTitle(ProcessedTitlesStorage storage) =>
            storage.RowKey ?? string.Empty;
    }
}