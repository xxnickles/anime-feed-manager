using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using Azure;
using Azure.Data.Tables;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage;

internal static class TableUtils
{
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
            return Right<DomainError, ImmutableList<T>>(resultList);

        }
        catch (Exception e)
        {
            return Left<DomainError, ImmutableList<T>>(ExceptionError.FromException(e, "TableQuery"));
        }
    }

    internal static async Task<Either<DomainError, T>> TryExecute<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            return e.Message == "Not Found" ?
                Left<DomainError, T>(NotFoundError.Create("TableOperation", "The entity was not found")) :
                Left<DomainError, T>(ExceptionError.FromException(e, "TableOperation"));
        }
    }

    internal static async Task<Either<DomainError, Unit>> BatchDelete<T>(TableClient tableClient, ImmutableList<T> entities) where T : ITableEntity
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
            return Left<DomainError, Unit>(ExceptionError.FromException(e, "BatchDelete"));
        }
    }
}