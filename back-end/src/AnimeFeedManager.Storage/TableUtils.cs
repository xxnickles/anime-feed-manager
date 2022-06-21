using AnimeFeedManager.Core.Error;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage;

internal class TableUtils
{
    internal static async Task<Either<DomainError, IImmutableList<T>>> TryGetAllTableElements<T>(CloudTable client, TableQuery<T> tableQuery) where T : TableEntity, new()
    {
        try
        {
            TableContinuationToken? token;
            var resultList = ImmutableList<T>.Empty;
            do
            {
                var result = await client.ExecuteQuerySegmentedAsync(tableQuery, null);
                resultList = resultList.AddRange(result.Results.AsEnumerable());
                token = result.ContinuationToken;
            } while (token != null);


            return Right<DomainError, IImmutableList<T>>(resultList);

        }
        catch (Exception e)
        {
            return Left<DomainError, IImmutableList<T>>(ExceptionError.FromException(e, "TableQuery"));
        }
    }

    internal static async Task<Either<DomainError, IImmutableList<T>>> TryGetAllTableElements<T>(CloudTable client) where T : TableEntity, new()
    {
        return await TryGetAllTableElements(client, new TableQuery<T>());
    }

    internal static async Task<Either<DomainError, IEnumerable<T>>> TryExecuteSimpleQuery<T>(
        Func<Task<TableQuerySegment<T>>> query) where T : TableEntity
    {
        try
        {
            var result = await query();

            return Right<DomainError, IEnumerable<T>>(result.Results.AsEnumerable());

        }
        catch (Exception e)
        {
            return Left<DomainError, IEnumerable<T>>(ExceptionError.FromException(e, "TableQuery"));
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

    internal static async Task<Either<DomainError, Unit>> BatchDelete<T>(CloudTable tableClient ,IImmutableList<T> entities) where T: TableEntity
    {
        try
        {
            var offset = 0;
            while (offset < entities.Count)
            {
                var batch = new TableBatchOperation();
                var rows = entities.Skip(offset).Take(100).ToList();
                foreach (var row in rows)
                {
                    batch.Delete(row);
                }

                await tableClient.ExecuteBatchAsync(batch);
                offset += rows.Count;
            }

            return Right(unit);
        }
        catch (Exception e)
        {
            return Left<DomainError, Unit>(ExceptionError.FromException(e, "BatchDelete"));
        }
    }
}