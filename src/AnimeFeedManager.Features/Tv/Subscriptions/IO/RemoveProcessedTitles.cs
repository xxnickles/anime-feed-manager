﻿using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IRemoveProcessedTitles
{
    Task<Either<DomainError, Unit>> Remove(DateTimeOffset time, CancellationToken token);
}

public class RemoveProcessedTitles : IRemoveProcessedTitles
{
    private readonly ITableClientFactory<ProcessedTitlesStorage> _clientFactory;

    public RemoveProcessedTitles(ITableClientFactory<ProcessedTitlesStorage> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public Task<Either<DomainError, Unit>> Remove(DateTimeOffset time, CancellationToken token)
    {
        return _clientFactory.GetClient()
            .BindAsync(client => GetItems(client, time, token))
            .BindAsync(param => Remove(param, token));
    }

    private static Task<Either<DomainError, (TableClient client, ImmutableList<ProcessedTitlesStorage> results)>> GetItems(
        TableClient client, DateTimeOffset time, CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<ProcessedTitlesStorage>(title => title.Timestamp <= time, cancellationToken: token))
            .MapAsync(results => (client, results));
    }

    private static Task<Either<DomainError, Unit>> Remove(
        (TableClient client, ImmutableList<ProcessedTitlesStorage> results) param, CancellationToken token)
    {
       return TableUtils.BatchDelete(param.client, param.results, token);
    } 
}