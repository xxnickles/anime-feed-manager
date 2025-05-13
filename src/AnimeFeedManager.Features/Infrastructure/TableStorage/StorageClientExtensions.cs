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

    internal static Task<Result<ImmutableList<T>>> TryToQuery<T>(this Task<Result<AppTableClient<T>>> clientResult,
        Func<TableClient,AsyncPageable<T>> query) where T : ITableEntity
    {
        return clientResult.Bind(tableClient => tableClient.ExecuteQuery(query));
    }

    internal static Task<Result<Unit>> WithDefaultMap(this Task<Result<Response>> result)
    {
        return result.Map(_ => new Unit());
    }
}