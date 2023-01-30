using System.Collections.Immutable;

namespace AnimeFeedManager.Storage;

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
        Func<AsyncPageable<T>> query, string typeName) where T : ITableEntity
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
                ? Right<DomainError, ImmutableList<T>>(resultList)
                : Left<DomainError, ImmutableList<T>>(NoContentError.Create($"TableQuery-{typeName}",
                    "Query returned no results"));
        }
        catch (Exception e)
        {
            return Left<DomainError, ImmutableList<T>>(ExceptionError.FromException(e, $"TableQuery-{typeName}"));
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
        Func<AsyncPageable<T>> query, string typeName) where T : ITableEntity
    {
        try
        {
            var queryResults = query();
            var resultList = ImmutableList<T>.Empty;
            await foreach (var qEntity in queryResults)
            {
                resultList = resultList.Add(qEntity);
            }

            return Right<DomainError, ImmutableList<T>>(resultList);
        }
        catch (Exception e)
        {
            return Left<DomainError, ImmutableList<T>>(ExceptionError.FromException(e, $"TableQuery-{typeName}"));
        }
    }


    
    /// <summary>
    /// Executes a query that iterates in a limited set of table pages at least once. Max records per page/request is 1000 <see cref="https://learn.microsoft.com/en-us/rest/api/storageservices/query-timeout-and-pagination"/>
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="typeName">Parameter Name</param>
    /// <param name="pageLimit">Maximum table pages to iterate</param>
    /// <typeparam name="T">Table Entity<</typeparam>
    /// <returns></returns>
    internal static async Task<Either<DomainError, ImmutableList<T>>> ExecuteLimitedQuery<T>(
        Func<AsyncPageable<T>> query, string typeName, byte pageLimit = 1)
    {
        var enumerator = query().GetAsyncEnumerator();
        var counter = 0;
        try
        {
            var resultList = ImmutableList<T>.Empty;
            while (await enumerator.MoveNextAsync() && counter < pageLimit)
            {
                resultList = resultList.Add(enumerator.Current);
                counter++;
            }

            return Right<DomainError, ImmutableList<T>>(resultList);
        }
        catch (Exception e)
        {
            return Left<DomainError, ImmutableList<T>>(ExceptionError.FromException(e, $"TableQuery-{typeName}"));
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
        
    }

    internal static async Task<Either<DomainError, T>> TryExecute<T>(Func<Task<T>> action, string typeName)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            return e.Message == "Not Found"
                ? Left<DomainError, T>(NotFoundError.Create($"TableOperation-{typeName}", "The entity was not found"))
                : Left<DomainError, T>(ExceptionError.FromException(e, $"TableOperation-{typeName}"));
        }
    }

    internal static async Task<Either<DomainError, Unit>> BatchDelete<T>(TableClient tableClient,
        ImmutableList<T> entities, string typeName) where T : ITableEntity
    {
        try
        {
            if (!entities.Any()) return Right(unit);
            var deleteEntitiesBatch = entities
                .Select(entityToDelete => new TableTransactionAction(TableTransactionActionType.Delete, entityToDelete))
                .ToList();

            await tableClient.SubmitTransactionAsync(deleteEntitiesBatch).ConfigureAwait(false);
            return Right(unit);
        }
        catch (Exception e)
        {
            return Left<DomainError, Unit>(ExceptionError.FromException(e, $"BatchDelete-{typeName}"));
        }
    }
}