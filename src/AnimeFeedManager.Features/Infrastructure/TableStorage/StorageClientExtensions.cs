using System.Linq.Expressions;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class StorageClientExtensions
{
    extension(TableClient client)
    {
        public async Task<Result<Response<T>, StorageError>> TryExecute<T>(Func<TableClient, Task<Response<T>>> action)
            where T : ITableEntity
        {
            try
            {
                return Result<Response<T>, StorageError>.Success(await action(client));
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return TableClient.HandleEntityException<Response<T>, T>(e);
            }
        }

        public async Task<Result<Response, StorageError>> TryExecute<T>(Func<TableClient, Task<Response>> action)
            where T : ITableEntity
        {
            try
            {
                return Result<Response, StorageError>.Success(await action(client));
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return TableClient.HandleEntityException<Response, T>(e);
            }
        }

        private static Result<TResult, StorageError> HandleEntityException<TResult, TEntity>(Exception e)
            where TEntity : ITableEntity => e switch
        {
            RequestFailedException { Status: 404 } or { Message: "Not Found" } => new StorageError.NotFound(typeof(TEntity)),
            RequestFailedException { Status: 412 } => new StorageError.Conflict(typeof(TEntity)),
            RequestFailedException => new StorageError.RequestFailed("table storage request", e),
            _ => new StorageError.RequestFailed("table client action", e)
        };

        public Task<Result<ImmutableArray<T>, StorageError>> ExecuteQuery<T>(Expression<Func<T, bool>> filter,
            CancellationToken token, IEnumerable<string>? select = null) where T : class, ITableEntity =>
            TableClient.CollectQuery(() => client.QueryAsync(filter, select: select, cancellationToken: token));

        /// <summary>
        /// Runs an OData filter built with <see cref="TableClient.CreateQueryFilter(FormattableString)"/>.
        /// Required for enum properties: they are stored as names, but a LINQ filter compares their numeric value.
        /// </summary>
        public Task<Result<ImmutableArray<T>, StorageError>> ExecuteQuery<T>(string filter,
            CancellationToken token, IEnumerable<string>? select = null) where T : class, ITableEntity =>
            TableClient.CollectQuery(() => client.QueryAsync<T>(filter, select: select, cancellationToken: token));

        private static async Task<Result<ImmutableArray<T>, StorageError>> CollectQuery<T>(Func<AsyncPageable<T>> query)
            where T : class, ITableEntity
        {
            try
            {
                var queryResults = query();
                var builder = ImmutableArray.CreateBuilder<T>();
                await foreach (var qEntity in queryResults)
                {
                    builder.Add(qEntity);
                }

                return Result<ImmutableArray<T>, StorageError>.Success(builder.DrainToImmutable());
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return new StorageError.RequestFailed("table query", e);
            }
        }

        public async Task<Result<Unit, StorageError>> AddBatch<T>(IEnumerable<T> entities,
            CancellationToken token,
            TableTransactionActionType actionType = TableTransactionActionType.UpsertMerge
        ) where T : ITableEntity
        {
            try
            {
                if (!entities.Any()) return Result<Unit, StorageError>.Success(new Unit());

                // Create batches based on partition keys, as this is a restriction in azure tables
                foreach (var groupedEntities in entities.GroupBy(e => e.PartitionKey))
                {
                    var actions = groupedEntities.Select(tableEntity =>
                        new TableTransactionAction(actionType, tableEntity));

                    foreach (var batch in actions.Chunk(99))
                    {
                        _ = await client.SubmitTransactionAsync(batch, token)
                            .ConfigureAwait(false);
                    }
                }

                return Result<Unit, StorageError>.Success(new Unit());
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return new StorageError.RequestFailed("batch write", e);
            }
        }
    }

    extension<T>(Task<Result<ImmutableArray<T>, StorageError>> result)
    {
        public Task<Result<T, StorageError>> SingleItem()
        {
            return result.Bind(x => x.IsEmpty
                ? new StorageError.NotFound(typeof(T))
                : Result<T, StorageError>.Success(x[0]));
        }

        public Task<Result<T?, StorageError>> SingleItemOrNull()
        {
            return result.Bind(x => x.IsEmpty
                ? Result<T?, StorageError>.Success(default)
                : Result<T?, StorageError>.Success(x[0]));
        }

    }

    internal static Task<Result<Unit, StorageError>> WithDefaultMap(this Task<Result<Response, StorageError>> result)
    {
        return result.Map(_ => new Unit());
    }
}