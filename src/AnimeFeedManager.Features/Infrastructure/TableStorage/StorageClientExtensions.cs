namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class StorageClientExtensions
{
    public static async Task<Result<Response<T>>> TryExecute<T>(
        this TableClient client,
        Func<TableClient, Task<Response<T>>> action) where T : ITableEntity
    {
        try
        {
            return Result<Response<T>>.Success(await action(client));
        }
        catch (RequestFailedException ex)
        {
            return ex.Status == 404
                ? NotFoundError.Create($"The entity of type {typeof(T).Name} was not found.")
                : ExceptionError.FromExceptionWithMessage(ex,
                    "An error occurred when executing a request to table storage service");
        }
        catch (Exception e)
        {
            return e.Message == "Not Found"
                ? NotFoundError.Create($"The entity of type {typeof(T).Name} was not found.")
                : ExceptionError.FromExceptionWithMessage(e, "An error occurred when executing a Table Client action");
        }
    }

    public static async Task<Result<Response>> TryExecute<T>(
        this TableClient client,
        Func<TableClient, Task<Response>> action) where T : ITableEntity
    {
        try
        {
            return Result<Response>.Success(await action(client));
        }
        catch (RequestFailedException ex)
        {
            return ex.Status == 404
                ? NotFoundError.Create($"The entity of type {typeof(T).Name} was not found.")
                : ExceptionError.FromExceptionWithMessage(ex,
                    "An error occurred when executing a request to table storage service");
        }
        catch (Exception e)
        {
            return e.Message == "Not Found"
                ? NotFoundError.Create($"The entity of type {typeof(T).Name} was not found.")
                : ExceptionError.FromExceptionWithMessage(e, "An error occurred when executing a Table Client action");
        }
    }

    public static async Task<Result<ImmutableList<T>>> ExecuteQuery<T>(this TableClient client,
        Func<TableClient, AsyncPageable<T>> query) where T : ITableEntity
    {
        try
        {
            var queryResults = query(client);
            var resultList = ImmutableList<T>.Empty;
            await foreach (var qEntity in queryResults)
            {
                resultList = resultList.Add(qEntity);
            }

            return Result<ImmutableList<T>>.Success(resultList);
        }
        catch (Exception e)
        {
            return ExceptionError.FromExceptionWithMessage(e, "An error occurred when executing a Table Client query");
        }
    }

    public static async Task<Result<Unit>> AddBatch<T>(
        this TableClient client,
        IEnumerable<T> entities,
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
                var addEntitiesBatch = new List<TableTransactionAction>();
                addEntitiesBatch.AddRange(
                    groupedEntities.Select(tableEntity =>
                        new TableTransactionAction(actionType, tableEntity)));
                const ushort limit = 99;
                for (ushort i = 0; i < addEntitiesBatch.Count; i += limit)
                {
                    _ = await client.SubmitTransactionAsync(addEntitiesBatch.Skip(i).Take(limit), token)
                        .ConfigureAwait(false);
                }
            }

            return Result<Unit>.Success();
        }
        catch (Exception e)
        {
            return ExceptionError.FromExceptionWithMessage(e, "An error occurred when executing add batch operation");
        }
    }

    public static Task<Result<T>> SingleItem<T>(this Task<Result<ImmutableList<T>>> result)
    {
        return result.Bind(x => x.IsEmpty
            ? NotFoundError.Create($"Item of  type '{typeof(T).FullName}' not found")
            : Result<T>.Success(x.First()));
    }

    public static Task<Result<T?>> SingleItemOrNull<T>(this Task<Result<ImmutableList<T>>> result)
    {
        return result.Bind(x => x.IsEmpty
            ? Result<T?>.Success(default)
            : Result<T?>.Success(x.First()));
    }

    public static Task<Result<T>> SingleItemOrNotFound<T>(this Task<Result<ImmutableList<T>>> result)
    {
        return result.Bind(x => x.IsEmpty
            ? NotFoundError.Create($"Not matches found for type {typeof(T).FullName}")
            : Result<T>.Success(x.First()));
    }

    internal static Task<Result<Unit>> WithDefaultMap(this Task<Result<Response>> result)
    {
        return result.Map(_ => new Unit());
    }
}