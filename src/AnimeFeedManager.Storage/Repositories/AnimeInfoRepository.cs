using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnimeFeedManager.Common.Helpers;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories
{
    public class AnimeInfoRepository : IAnimeInfoRepository
    {
        private readonly CloudTable _tableClient;

        public AnimeInfoRepository(ITableClientFactory<AnimeInfoStorage> tableClientFactory)
        {
            _tableClient = tableClientFactory.GetCloudTable();
        }

        public Task<Either<DomainError, IEnumerable<AnimeInfoWithImageStorage>>> GetBySeason(Season season, int year)
        {
            var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, (ushort)year);
            var tableQuery = new TableQuery<AnimeInfoWithImageStorage>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return TableUtils.TryExecuteSimpleQuery(() => _tableClient.ExecuteQuerySegmentedAsync(tableQuery, null));
        }

        public async Task<Either<DomainError, Unit>> Merge(AnimeInfoStorage animeInfo)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(animeInfo);
            var result = await TableUtils.TryExecute(async () =>
            {
                await _tableClient.CreateIfNotExistsAsync();
                return _tableClient.ExecuteAsync(insertOrMergeOperation);
            });
            return result.Map(r => unit);
        }

        public async Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(image);
            var result = await TableUtils.TryExecute(async () =>
            {
                await TableUtils.TryExecute(() => _tableClient.ExecuteAsync(insertOrMergeOperation));
                return _tableClient.ExecuteAsync(insertOrMergeOperation);
            });
            return result.Map(r => unit);
        }
    }
}
