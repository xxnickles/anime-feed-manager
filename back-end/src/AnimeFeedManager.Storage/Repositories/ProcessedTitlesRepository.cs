using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using Azure;
using Azure.Data.Tables;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories;

public class ProcessedTitlesRepository : IProcessedTitlesRepository
{
    private readonly TableClient _tableClient;
    public ProcessedTitlesRepository(ITableClientFactory<ProcessedTitlesStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
    }

    public async Task<Either<DomainError, ImmutableList<string>>> GetProcessedTitles()
    {
        return await TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<ProcessedTitlesStorage>()).MapAsync(Map);
    }

    public Task<Either<DomainError, Unit>> RemoveExpired()
    {
        var result = TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<ProcessedTitlesStorage>(t =>
                t.PartitionKey == "feed-processed" &&
                t.Timestamp <= DateTimeOffset.Now));

        return result.BindAsync(BatchDelete);
    }

    public async Task<Either<DomainError, Unit>> Merge(ProcessedTitlesStorage processedTitles)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpdateEntityAsync(processedTitles, ETag.All));
        return result.Map(_ => unit);
    }

    private Task<Either<DomainError, Unit>> BatchDelete(ImmutableList<ProcessedTitlesStorage> titles)
    {
        return TableUtils.BatchDelete(_tableClient, titles);
    }

    private static ImmutableList<string> Map(ImmutableList<ProcessedTitlesStorage> titles)
    {
        return titles.Select(storageTitle => storageTitle.Title ?? string.Empty).ToImmutableList();
    }
}