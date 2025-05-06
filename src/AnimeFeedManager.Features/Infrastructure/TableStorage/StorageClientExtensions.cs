namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class StorageClientExtensions
{
    private static async Task<Result<T>> TryExecute<T>(
        this TableClient client,
        Func<TableClient, Task<T>> action,
        ILogger logger)
    {
        try
        {
            return Result<T>.Success(await action(client));
        } catch (RequestFailedException ex)
        {
            logger.LogError(ex, "An error occurred when executing a request to table storage service");
            return ex.Status == 404 ? 
                NotFoundResult<T>($"The entity of type {typeof(T).Name} was not found.") :
                HandledErrorResult<T>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred when executing a Table Client action");
            return e.Message == "Not Found" ?
                NotFoundResult<T>($"The entity of type {typeof(T).Name} was not found.") :
                HandledErrorResult<T>();
        }
        
    }

    internal static  Task<Result<T>> TryExecute<T>(this Task<Result<TableClient>> clientResult,
        Func<TableClient, Task<T>> action, ILogger logger)
    {
        return clientResult.Bind(tableClient => tableClient.TryExecute(action, logger));
    }

    internal static Task<Result<Unit>> WithDefaultMap(this Task<Result<Response>> result)
    {
        return result.Map(_ => new Unit());
    }
}

