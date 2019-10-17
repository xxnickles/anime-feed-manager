using AnimeFeedManager.Core.Error;
using LanguageExt;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage
{
    internal class TableUtils
    {
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
                return Left<DomainError, T>(ExceptionError.FromException(e, "TableOperation"));
            }
        }
    }
}
