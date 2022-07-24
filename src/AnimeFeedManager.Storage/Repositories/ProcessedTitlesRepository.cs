using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class ProcessedTitlesRepository : IProcessedTitlesRepository
{
    private readonly TableClient _tableClient;

    public ProcessedTitlesRepository(ITableClientFactory<ProcessedTitlesStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task<Either<DomainError, ImmutableList<string>>> GetProcessedTitles()
    {
        return await TableUtils
            .ExecuteQueryWithEmpty(() => _tableClient.QueryAsync<ProcessedTitlesStorage>(), nameof(ProcessedTitlesStorage))
            .MapAsync(Map);
    }

    public Task<Either<DomainError, Unit>> RemoveExpired()
    {
        var result = TableUtils.ExecuteQueryWithEmpty(() =>
            _tableClient.QueryAsync<ProcessedTitlesStorage>(t =>
                t.PartitionKey == "feed-processed" &&
                t.Timestamp <= DateTimeOffset.Now), nameof(ProcessedTitlesStorage));

        return result.BindAsync(BatchDelete);
    }

    public async Task<Either<DomainError, Unit>> Merge(ProcessedTitlesStorage processedTitles)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(processedTitles),
            nameof(ProcessedTitlesStorage));
        return result.Map(_ => unit);
    }

    private Task<Either<DomainError, Unit>> BatchDelete(ImmutableList<ProcessedTitlesStorage> titles)
    {
        return TableUtils.BatchDelete(_tableClient, titles, nameof(ProcessedTitlesStorage));
    }

    private static ImmutableList<string> Map(ImmutableList<ProcessedTitlesStorage> titles)
    {
        return titles.Select(storageTitle => storageTitle.Title ?? string.Empty).ToImmutableList();
    }
}