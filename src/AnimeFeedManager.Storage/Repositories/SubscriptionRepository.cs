using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly CloudTable _tableClient;

        public SubscriptionRepository(ITableClientFactory<SubscriptionStorage> tableClientFactory) => _tableClient = tableClientFactory.GetCloudTable();

        public async Task<Either<DomainError, IEnumerable<SubscriptionStorage>>> GetAll()
        {
            var tableQuery = new TableQuery<SubscriptionStorage>();
            return await TableUtils.TryExecuteSimpleQuery(() => _tableClient.ExecuteQuerySegmentedAsync(tableQuery, null));
        }

        public async Task<Either<DomainError, SubscriptionStorage>> Merge(SubscriptionStorage subscription)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(subscription);
            var result = await TableUtils.TryExecute(() => _tableClient.ExecuteAsync(insertOrMergeOperation));
            return result.Map(r => (SubscriptionStorage)r.Result);
        }
    }
}
