using System.Collections.Immutable;

namespace AnimeFeedManager.Storage;

internal static class TableUtils
{
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
                : Left<DomainError, ImmutableList<T>>(NotFoundError.Create($"TableQuery-{typeName}",
                    "Query returned no results"));
        }
        catch (Exception e)
        {
            return Left<DomainError, ImmutableList<T>>(ExceptionError.FromException(e, $"TableQuery-{typeName}"));
        }
    }

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