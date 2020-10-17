using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Immutable;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly CloudTable _tableClient;

        public SubscriptionRepository(ITableClientFactory<SubscriptionStorage> tableClientFactory) => _tableClient = tableClientFactory.GetCloudTable();

        public async Task<Either<DomainError, IImmutableList<SubscriptionStorage>>> Get(Email userEmail)
        {
            var user = OptionUtils.UnpackOption(userEmail.Value, string.Empty);
            var tableQuery = new TableQuery<SubscriptionStorage>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, user));
           
            return await TableUtils.TryGetAllTableElements(_tableClient, tableQuery); ;
        }

        public async Task<Either<DomainError, IImmutableList<SubscriptionStorage>>> GetAll()
        {
            return await TableUtils.TryGetAllTableElements<SubscriptionStorage>(_tableClient);
        }

        public async Task<Either<DomainError, SubscriptionStorage>> Merge(SubscriptionStorage subscription)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(subscription);
            var result = await TableUtils.TryExecute(() => _tableClient.ExecuteAsync(insertOrMergeOperation));
            return result.Map(r => (SubscriptionStorage)r.Result);
        }

        public async Task<Either<DomainError, Unit>> Delete(SubscriptionStorage subscription)
        {
            TableOperation deleteOperation = TableOperation.Delete(subscription.AddEtag());
            var result = await TableUtils.TryExecute(() => _tableClient.ExecuteAsync(deleteOperation));
            return result.Map(x => unit);
        }
    }
}
