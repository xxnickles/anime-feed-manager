using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeFeedManager.Storage.Repositories;

public class ProcessedTitlesRepository: IProcessedTitlesRepository
{

    private readonly CloudTable _tableClient;
    public ProcessedTitlesRepository(ITableClientFactory<ProcessedTitlesStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetCloudTable();
    }

    public async Task<Either<DomainError, IImmutableList<string>>> GetProcessedTitles()
    {
        return await TableUtils.TryGetAllTableElements<ProcessedTitlesStorage>(_tableClient).MapAsync(Map);
    }
    public Task<Either<DomainError, Unit>> RemoveExpired()
    {
        var filterByPartition = TableQuery.GenerateFilterCondition("PartitionKey",
            QueryComparisons.Equal, "feed-processed");

        var filterByDate = TableQuery.GenerateFilterConditionForDate("Timestamp",
            QueryComparisons.LessThanOrEqual, DateTimeOffset.Now);

        var queryFilter = TableQuery.CombineFilters(filterByPartition, TableOperators.And, filterByDate);
        var tableQuery = new TableQuery<ProcessedTitlesStorage>().Where(queryFilter);
        var result =  TableUtils.TryGetAllTableElements(_tableClient, tableQuery);
        return result.BindAsync(BatchDelete);
    }

    private IImmutableList<string> Map(IImmutableList<ProcessedTitlesStorage> titles)
    {
        return titles.Select(storageTitle => storageTitle.Title ?? string.Empty).ToImmutableList();
    }

    private Task<Either<DomainError, Unit>> BatchDelete(IImmutableList<ProcessedTitlesStorage> titles)
    {
        return TableUtils.BatchDelete(_tableClient, titles);
    }
}