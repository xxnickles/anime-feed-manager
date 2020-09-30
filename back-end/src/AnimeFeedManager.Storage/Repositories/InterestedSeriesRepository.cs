using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories
{
    public class InterestedSeriesRepository : IInterestedSeriesRepository
    {
        private readonly CloudTable _tableClient;
        public InterestedSeriesRepository(ITableClientFactory<InterestedStorage> tableClientFactory)
        {
            _tableClient = tableClientFactory.GetCloudTable();
        }

        public async Task<Either<DomainError, IImmutableList<InterestedStorage>>> GetAll()
        {
            return await TableUtils.TryGetAllTableElements<InterestedStorage>(_tableClient);
        }

        public async Task<Either<DomainError, IEnumerable<InterestedStorage>>> Get(Email userEmail)
        {
            var user = OptionUtils.UnpackOption(userEmail.Value, string.Empty);
            var tableQuery = new TableQuery<InterestedStorage>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, user));
            return await TableUtils.TryExecuteSimpleQuery(() => _tableClient.ExecuteQuerySegmentedAsync(tableQuery, null));
           
        }

        public async Task<Either<DomainError, InterestedStorage>> Merge(InterestedStorage subscription)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(subscription);
            var result = await TableUtils.TryExecute(() => _tableClient.ExecuteAsync(insertOrMergeOperation));
            return result.Map(r => (InterestedStorage)r.Result);
        }

        public async Task<Either<DomainError, Unit>> Delete(InterestedStorage subscription)
        {
            TableOperation deleteOperation = TableOperation.Delete(subscription.AddEtag());
            var result = await TableUtils.TryExecute(() => _tableClient.ExecuteAsync(deleteOperation));
            return result.Map(x => unit);
        }
    }
}
