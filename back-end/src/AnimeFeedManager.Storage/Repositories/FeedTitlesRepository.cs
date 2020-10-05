using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories
{
    public class FeedTitlesRepository : IFeedTitlesRepository
    {
        private readonly CloudTable _tableClient;

        public FeedTitlesRepository(ITableClientFactory<TitlesStorage> tableClientFactory)
        {
            _tableClient = tableClientFactory.GetCloudTable();
        }

        public Task<Either<DomainError, IImmutableList<string>>> GetTitles()
        {
            var tableQuery = new TableQuery<TitlesStorage>();
            return TableUtils.TryExecuteSimpleQuery(() => _tableClient.ExecuteQuerySegmentedAsync(tableQuery, null))
                     .BindAsync(Mapper);

        }

        public async Task<Either<DomainError, Unit>> MergeTitles(IEnumerable<string> titles)
        {
            var titleStorage = new TitlesStorage {Titles = string.Join(',', titles), PartitionKey = "feed_titles", RowKey = "standard" };
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(titleStorage.AddEtag());
            var result = await TableUtils.TryExecute(() => _tableClient.ExecuteAsync(insertOrMergeOperation));
            return result.Map(r => unit);
        }

        private Either<DomainError, IImmutableList<string>> Mapper(IEnumerable<TitlesStorage> source)
        {
            try
            {
                if (!source.Any()) return ImmutableList<string>.Empty;
                var item = source.Single();
                if (item == null || string.IsNullOrWhiteSpace(item.Titles))
                {
                    return BasicError.Create(nameof(FeedTitlesRepository), "Title source contains more than one record");
                }

                return item
                    .Titles
                    .Split(',')
                    .Select(x => x.Trim())
                    .ToImmutableList();

            }
            catch (Exception e)
            {
                return ExceptionError.FromException(e);
            }
        }
    }
}