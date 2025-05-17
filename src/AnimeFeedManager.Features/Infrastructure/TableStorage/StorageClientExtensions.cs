namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class StorageClientExtensions
{
    private static async Task<Result<T>> TryExecute<T>(
        this AppTableClient<T> client,
        Func<TableClient, Task<T>> action) where T : ITableEntity
    {
        try
        {
            return Result<T>.Success(await action(client.Client));
        }
        catch (RequestFailedException ex)
        {
            client.Logger.LogError(ex, "An error occurred when executing a request to table storage service");
            return ex.Status == 404
                ? NotFoundResult<T>($"The entity of type {typeof(T).Name} was not found.")
                : HandledErrorResult<T>();
        }
        catch (Exception e)
        {
            client.Logger.LogError(e, "An error occurred when executing a Table Client action");
            return e.Message == "Not Found"
                ? NotFoundResult<T>($"The entity of type {typeof(T).Name} was not found.")
                : HandledErrorResult<T>();
        }
    }

    internal static Task<Result<T>> TryExecute<T>(this Task<Result<AppTableClient<T>>> clientResult,
        Func<TableClient, Task<T>> action) where T : ITableEntity
    {
        return clientResult.Bind(tableClient => tableClient.TryExecute(action));
    }

    public static async Task<Result<ImmutableList<T>>> ExecuteQuery<T>(this AppTableClient<T> client,
        Func<TableClient,AsyncPageable<T>> query) where T : ITableEntity
    {
        try
        {
            var queryResults = query(client.Client);
            var resultList = ImmutableList<T>.Empty;
            await foreach (var qEntity in queryResults)
            {
                resultList = resultList.Add(qEntity);
            }
            
            return Result<ImmutableList<T>>.Success(resultList);
        }
        catch (Exception e)
        {
            client.Logger.LogError(e, "An error occurred when executing a Table Client query");
            return HandledErrorResult<ImmutableList<T>>();
        }
    }

    public static async Task<Result<Unit>> AddBatch<T>(
        this AppTableClient<T> client,
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
                    _ = await client.Client.SubmitTransactionAsync(addEntitiesBatch.Skip(i).Take(limit), token)
                        .ConfigureAwait(false);
                }
            }

            return Result<Unit>.Success();
        }
        catch (Exception e)
        {
            client.Logger.LogError(e, "An error occurred when executing add batch operation");
            return HandledErrorResult<Unit>();
        }
    }


    internal static Task<Result<Unit>> WithDefaultMap(this Task<Result<Response>> result)
    {
        return result.Map(_ => new Unit());
    }
}