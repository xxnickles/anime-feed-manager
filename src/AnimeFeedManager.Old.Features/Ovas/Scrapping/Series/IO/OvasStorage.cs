﻿using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;

public interface IOvasStorage
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<OvaStorage> series, CancellationToken token);

    Task<Either<DomainError, Unit>> RemoveOva(RowKey rowKey, PartitionKey key, CancellationToken token);

    Task<Either<DomainError, Unit>> Update(OvaStorage series, CancellationToken token);
}

public sealed class OvasStorage(ITableClientFactory<OvaStorage> tableClientFactory) : IOvasStorage
{
    public Task<Either<DomainError, Unit>> Add(ImmutableList<OvaStorage> series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series, token))
            .MapAsync(_ => unit);
    }

    public Task<Either<DomainError, Unit>> RemoveOva(RowKey rowKey, PartitionKey key, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() =>
                client.DeleteEntityAsync(key, rowKey, cancellationToken: token)))
            .MapAsync(_ => unit);
    }

    public Task<Either<DomainError, Unit>> Update(OvaStorage series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.TryExecute(() => client.UpsertEntityAsync(series, TableUpdateMode.Merge, token)))
            .MapAsync(_ => unit);
    }
}