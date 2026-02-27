using System.Linq.Expressions;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class StorageClientExtensions
{
    extension(TableClient client)
    {
        public async Task<Result<Response<T>>> TryExecute<T>(Func<TableClient, Task<Response<T>>> action)
            where T : ITableEntity
        {
            try
            {
                return Result<Response<T>>.Success(await action(client));
            }
            catch (Exception e)
            {
                return TableClient.HandleEntityException<Response<T>, T>(e);
            }
        }

        public async Task<Result<Response>> TryExecute<T>(Func<TableClient, Task<Response>> action)
            where T : ITableEntity
        {
            try
            {
                return Result<Response>.Success(await action(client));
            }
            catch (Exception e)
            {
                return TableClient.HandleEntityException<Response, T>(e);
            }
        }

        private static Result<TResult> HandleEntityException<TResult, TEntity>(Exception e)
            where TEntity : ITableEntity => e switch
        {
            RequestFailedException { Status: 404 } =>
                NotFoundError.Create($"The entity of type {typeof(TEntity).Name} was not found."),
            { Message: "Not Found" } =>
                NotFoundError.Create($"The entity of type {typeof(TEntity).Name} was not found."),
            RequestFailedException =>
                ExceptionError.FromExceptionWithMessage(e,
                    "An error occurred when executing a request to table storage service"),
            _ => ExceptionError.FromExceptionWithMessage(e,
                "An error occurred when executing a Table Client action")
        };

        public async Task<Result<ImmutableArray<T>>> ExecuteQuery<T>(Expression<Func<T, bool>> filter,
            CancellationToken token, IEnumerable<string>? select = null) where T : class, ITableEntity
        {
            try
            {
                var queryResults = client.QueryAsync(filter, select: select, cancellationToken: token);
                var builder = ImmutableArray.CreateBuilder<T>();
                await foreach (var qEntity in queryResults)
                {
                    builder.Add(qEntity);
                }

                return Result<ImmutableArray<T>>.Success(builder.DrainToImmutable());
            }
            catch (Exception e)
            {
                return ExceptionError.FromExceptionWithMessage(e,
                    "An error occurred when executing a Table Client query");
            }
        }

        public async Task<Result<Unit>> AddBatch<T>(IEnumerable<T> entities,
            CancellationToken token,
            TableTransactionActionType actionType = TableTransactionActionType.UpsertMerge
        ) where T : ITableEntity
        {
            try
            {
                if (!entities.Any()) return Result<Unit>.Success();

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

                return Result<Unit>.Success();
            }
            catch (Exception e)
            {
                return ExceptionError.FromExceptionWithMessage(e,
                    "An error occurred when executing add batch operation");
            }
        }
    }

    extension<T>(Task<Result<ImmutableArray<T>>> result)
    {
        public Task<Result<T>> SingleItem()
        {
            return result.Bind(x => x.IsEmpty
                ? NotFoundError.Create($"Item of type '{typeof(T).FullName}' not found")
                : Result<T>.Success(x[0]));
        }

        public Task<Result<T?>> SingleItemOrNull()
        {
            return result.Bind(x => x.IsEmpty
                ? Result<T?>.Success(default)
                : Result<T?>.Success(x[0]));
        }

    }

    internal static Task<Result<Unit>> WithDefaultMap(this Task<Result<Response>> result)
    {
        return result.Map(_ => new Unit());
    }
}