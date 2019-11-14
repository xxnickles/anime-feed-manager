using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeFeedManager.Storage.Repositories
{
    public class AnimeInfoRepository : IAnimeInfoRepository
    {
        private readonly CloudTable _tableClient;

        public AnimeInfoRepository(ITableClientFactory<AnimeInfoStorage> tableClientFactory) => _tableClient = tableClientFactory.GetCloudTable();

        public Task<Either<DomainError, IEnumerable<AnimeInfoStorage>>> GetBySeason(Season season, int year)
        {
            var partitionKey = $"{season.Value}-{year.ToString()}";
            var tableQuery = new TableQuery<AnimeInfoStorage>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return TableUtils.TryExecuteSimpleQuery(() => _tableClient.ExecuteQuerySegmentedAsync(tableQuery, null));
        }
    }
}
