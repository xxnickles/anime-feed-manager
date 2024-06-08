using System.Runtime.CompilerServices;
using AnimeFeedManager.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class TableUtils
{
    /// <summary>
    /// Executes a query. Returns Domain Error on empty results
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter name</param>
    /// <param name="callerPath">Caller Path</param>
    /// <param name="callerName">Caller Name</param>
    /// <typeparam name="T">Table Entity</typeparam>
    /// <returns>Error or Immutable list of <typeparamref name="T"/> (only when there are results)</returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteQuery<T>(
        Func<AsyncPageable<T>> query,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
    {
        try
        {
            var queryResults = query();
            var resultList = ImmutableList<T>.Empty;
            await foreach (var qEntity in queryResults)
            {
                resultList = resultList.Add(qEntity);
            }

            return !resultList.IsEmpty
                ? resultList
                : NoContentError.Create(
                    $"Query for the entity '{typeof(T).Name}' returned no results. {CallerInfo(callerPath, callerName)}");
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
    }

    /// <summary>
    /// Executes a query with default empty results
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter name</param>
    /// <param name="callerPath">Caller Path</param>
    /// <param name="callerName">Caller Name</param>
    /// <typeparam name="T">Table Entity</typeparam>
    /// <returns>Error or Immutable list of <typeparamref name="T"/></returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteQueryWithNotFoundResult<T>(
        Func<AsyncPageable<T>> query,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
    {
        try
        {
            var queryResults = query();
            var resultList = ImmutableList<T>.Empty;
            await foreach (var qEntity in queryResults)
            {
                resultList = resultList.Add(qEntity);
            }

            return !resultList.IsEmpty
                ? resultList
                : NotFoundError.Create(
                    $"Query for the entity '{typeof(T).Name}' returned no results. {CallerInfo(callerPath, callerName)}");
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
    }


    /// <summary>
    /// Executes a query with that return no found instead of no content when no items match
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter name</param>
    /// <param name="callerPath">Caller Path</param>
    /// <param name="callerName">Caller Name</param>
    /// <typeparam name="T">Table Entity</typeparam>
    /// <returns>Error or Immutable list of <typeparamref name="T"/></returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteQueryWithEmptyResult<T>(
        Func<AsyncPageable<T>> query,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
    {
        try
        {
            var queryResults = query();
            var resultList = ImmutableList<T>.Empty;
            await foreach (var qEntity in queryResults)
            {
                resultList = resultList.Add(qEntity);
            }

            return resultList;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
    }


    /// <summary>
    /// Executes a query that iterates in a limited set of records at least once. Max records per page/request is 1000 <see cref="https://learn.microsoft.com/en-us/rest/api/storageservices/query-timeout-and-pagination"/>
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter Name</param>
    /// <param name="items">Maximum items</param>
    ///  <param name="callerPath">Caller Path</param>
    /// <param name="callerName">Caller Name</param>
    /// <typeparam name="T">Table Entity</typeparam>
    /// <returns></returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteLimitedQuery<T>(
        Func<AsyncPageable<T>> query, byte items = 1,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
    {
        var enumerator = query().GetAsyncEnumerator();
        var counter = 0;
        try
        {
            var resultList = ImmutableList<T>.Empty;
            while (await enumerator.MoveNextAsync() && counter < items)
            {
                resultList = resultList.Add(enumerator.Current);
                counter++;
            }

            return resultList;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    /// <summary>
    /// Run a request, but it doesn't fail in case of the given entity/item is not found in the storage
    /// </summary>
    /// <param name="action"></param>
    /// <param name="fallbackValue"></param>
    /// <param name="callerPath"></param>
    /// <param name="callerName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal static async Task<Either<DomainError, T>> ExecuteWithFallback<T>(Func<Task<T>> action, T fallbackValue,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "")
    {
        try
        {
            return await action();
        }
        catch (RequestFailedException)
        {
            return fallbackValue;
        }
        catch (Exception e)
        {
            return e.Message == "Not Found"
                ? fallbackValue
                : ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
    }

    internal static async Task<Either<DomainError, T>> TryExecute<T>(Func<Task<T>> action,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "")
    {
        try
        {
            return await action();
        }
        catch (RequestFailedException ex)
        {
            return ex.Status == 404
                ? NotFoundError.Create(
                    $"The entity of type {typeof(T).Name} was not found. {CallerInfo(callerPath, callerName)}")
                : ExceptionError.FromException(ex, CallerInfo(callerPath, callerName));
        }
        catch (Exception e)
        {
            return e.Message == "Not Found"
                ? NotFoundError.Create(
                    $"The entity of type {typeof(T).Name} was not found. {CallerInfo(callerPath, callerName)}")
                : ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
    }

    internal static async Task<Either<DomainError, Unit>> BatchDelete<T>(TableClient tableClient,
        ImmutableList<T> entities, CancellationToken token,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
    {
        try
        {
            if (entities.IsEmpty) return unit;
            // Create batches based on partition keys, as this is a restriction in azure tables
            foreach (var groupedEntities in entities.GroupBy(e => e.PartitionKey))
            {
                var deleteEntitiesBatch = groupedEntities
                    .Select(entityToDelete =>
                        new TableTransactionAction(TableTransactionActionType.Delete, entityToDelete))
                    .ToList();

                const ushort limit = 99;
                for (ushort i = 0; i < deleteEntitiesBatch.Count; i += limit)
                {
                    _ = await tableClient.SubmitTransactionAsync(deleteEntitiesBatch.Skip(i).Take(limit), token)
                        .ConfigureAwait(false);
                }
            }

            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
    }

    internal static async Task<Either<DomainError, Unit>> BatchAdd<T>(TableClient tableClient,
        IEnumerable<T> entities, CancellationToken token,
        TableTransactionActionType actionType = TableTransactionActionType.UpsertMerge,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string callerName = "") where T : ITableEntity
    {
        try
        {
            if (!entities.Any()) return unit;

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
                    _ = await tableClient.SubmitTransactionAsync(addEntitiesBatch.Skip(i).Take(limit), token)
                        .ConfigureAwait(false);
                }
            }

            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e, CallerInfo(callerPath, callerName));
        }
    }

    private static string CallerInfo(string callerPath, string callerName) =>
        $"Called from {callerPath} by {callerName}";
}