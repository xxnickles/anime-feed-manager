using AnimeFeedManager.Features.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class TableUtils
{
    /// <summary>
    /// Executes a query. Returns Domain Error on empty results
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter name</param>
    /// <typeparam name="T">Table Entity</typeparam>
    /// <returns>Error or Immutable list of <typeparamref name="T"/> (only when there are results)</returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteQuery<T>(
        Func<AsyncPageable<T>> query) where T : ITableEntity
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
                : NoContentError.Create($"Query for the entity '{typeof(T).Name}' returned no results");
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    /// <summary>
    /// Executes a query with default empty results
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter name</param>
    /// <typeparam name="T">Table Entity</typeparam>
    /// <returns>Error or Immutable list of <typeparamref name="T"/></returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteQueryWithEmpty<T>(
        Func<AsyncPageable<T>> query) where T : ITableEntity
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
            return ExceptionError.FromException(e);
        }
    }


    /// <summary>
    /// Executes a query that iterates in a limited set of records at least once. Max records per page/request is 1000 <see cref="https://learn.microsoft.com/en-us/rest/api/storageservices/query-timeout-and-pagination"/>
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter Name</param>
    /// <param name="items">Maximum items</param>
    /// <typeparam name="T">Table Entity<</typeparam>
    /// <returns></returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteLimitedQuery<T>(
        Func<AsyncPageable<T>> query, byte items = 1) where T : notnull
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
            return ExceptionError.FromException(e);
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    internal static async Task<Either<DomainError, T>> TryExecute<T>(Func<Task<T>> action )
    {
        try
        {
            return await action();
        }
        catch (RequestFailedException _)
        {
            return NotFoundError.Create($"The entity of type {typeof(T).Name} was not found");
        }
        catch (Exception e)
        {
            return e.Message == "Not Found"
                ? NotFoundError.Create($"The entity of type {typeof(T).Name} was not found")
                : ExceptionError.FromException(e);
        }
    }

    internal static async Task<Either<DomainError, Unit>> BatchDelete<T>(TableClient tableClient,
        ImmutableList<T> entities, CancellationToken token) where T : ITableEntity
    {
        try
        {
            if (!entities.Any()) return unit;
            var deleteEntitiesBatch = entities
                .Select(entityToDelete => new TableTransactionAction(TableTransactionActionType.Delete, entityToDelete))
                .ToList();

            await tableClient.SubmitTransactionAsync(deleteEntitiesBatch, token).ConfigureAwait(false);
            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    internal static async Task<Either<DomainError, Unit>> BatchAdd<T>(TableClient tableClient,
        ImmutableList<T> entities, CancellationToken token) where T : ITableEntity
    {
        try
        {
            // Create the batch.
            var addEntitiesBatch = new List<TableTransactionAction>();
            addEntitiesBatch.AddRange(
                entities.Select(e => new TableTransactionAction(TableTransactionActionType.UpsertMerge, e)));
            
            var response = await tableClient.SubmitTransactionAsync(addEntitiesBatch, token).ConfigureAwait(false);
            return unit;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}