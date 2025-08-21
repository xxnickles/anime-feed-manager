namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class StorageClientExtensions
{
    public static async Task<Result<Response>> TryExecute<T>(
        this AppTableClient client,
        Func<TableClient, Task<Response>> action,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
    {
        try
        {
            return Result<Response>.Success(await action(client.Client));
        }
        catch (RequestFailedException ex)
        {
            var context = GetContextInfo<T>(callerPath, callerName);
            client.Logger.LogError(ex, "An error occurred when executing a request to table storage service. {@Context}", context);
            return ex.Status == 404
                ? NotFoundResult<Response>($"The entity of type {typeof(T).Name} was not found.")
                : HandledErrorResult<Response>();
        }
        catch (Exception e)
        {
            var context = GetContextInfo<T>(callerPath, callerName);
            client.Logger.LogError(e, "An error occurred when executing a Table Client action. {@Context}", context);
            return e.Message == "Not Found"
                ? NotFoundResult<Response>($"The entity of type {typeof(T).Name} was not found.")
                : HandledErrorResult<Response>();
        }
    }
    
    public static async Task<Result<ImmutableList<T>>> ExecuteQuery<T>(this AppTableClient client,
        Func<TableClient, AsyncPageable<T>> query,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
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
            var context = GetContextInfo<T>(callerPath, callerName);
            client.Logger.LogError(e, "An error occurred when executing a Table Client query. {@Context}", context);
            return HandledErrorResult<ImmutableList<T>>();
        }
    }

    public static async Task<Result<Unit>> AddBatch<T>(
        this AppTableClient client,
        IEnumerable<T> entities,
        CancellationToken token,
        TableTransactionActionType actionType = TableTransactionActionType.UpsertMerge,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = ""
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
            var context = GetContextInfo<T>(callerPath, callerName, entities.Count());
            client.Logger.LogError(e, "An error occurred when executing add batch operation. {@Context}", context);
            return HandledErrorResult<Unit>();
        }
    }

    public static Task<Result<T>> SingleItem<T>(this Task<Result<ImmutableList<T>>> result)
    {
        return result.Bind(x => x.IsEmpty
            ? Error.Create($"The collection of type {typeof(T).FullName} is empty")
            : Result<T>.Success(x.First()));
    }

    internal static Task<Result<Unit>> WithDefaultMap(this Task<Result<Response>> result)
    {
        return result.Map(_ => new Unit());
    }

    private static object GetContextInfo<T>(string callerPath, string callerName, int? batchCount = null)
    {
        var fileName = Path.GetFileNameWithoutExtension(callerPath);
        var entityType = typeof(T).Name;
        
        return batchCount.HasValue
            ? new { EntityType = entityType, CallerFile = fileName, CallerMethod = callerName, BatchSize = batchCount.Value }
            : new { EntityType = entityType, CallerFile = fileName, CallerMethod = callerName };
    }
}