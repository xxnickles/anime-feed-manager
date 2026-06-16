using System.Collections.Immutable;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Infrastructure.Cosmos.Types;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AnimeFeedManager.Infrastructure.Cosmos.Static;

public static class ContainerExtensionMembers
{
    extension<TEntity>(IQueryable<TEntity> query) where TEntity : CosmosDocument
    {
        private IQueryable<TEntity> ApplyPredicates(CosmosQuerySpecification<TEntity> spec)
        {
            return spec.Predicates().Aggregate(query, (current, predicate) => current.Where(predicate));
        }

        private IQueryable<TEntity> ApplyOrdering(CosmosQuerySpecification<TEntity> spec)
        {
            if (spec.SortBy is null) return query;

            var field = spec.SortableFields
                .FirstOrDefault(kvp => string.Equals(kvp.Key, spec.SortBy, StringComparison.OrdinalIgnoreCase))
                .Value;

            return field is not null ? query.ApplySort(field, spec.SortDir ?? SortDirection.Ascending) : query;
        }

        private IQueryable<TEntity> ApplySort(SortableField<TEntity> field, SortDirection direction)
        {
            return field.Apply(query, direction);
        }

        /// <summary>
        /// Pushes the spec's <see cref="CosmosQuerySpecification{TEntity}.MaxItems"/> down to Cosmos
        /// as a SQL <c>TOP N</c> — server-side cap on the total items returned. A non-positive
        /// <c>MaxItems</c> (e.g. <c>-1</c>) is treated as unbounded and skipped.
        /// </summary>
        private IQueryable<TEntity> ApplyMaxItems(CosmosQuerySpecification<TEntity> spec)
        {
            return spec.MaxItems > 0 ? query.Take(spec.MaxItems) : query;
        }
    }

    private static QueryRequestOptions BuildRequestOptions<TEntity>(CosmosQuerySpecification<TEntity> spec)
        where TEntity : CosmosDocument
    {
        var options = new QueryRequestOptions { MaxItemCount = spec.MaxItems };

        if (spec.PartitionKey is { } pk)
            options.PartitionKey = pk;

        return options;
    }

    extension(Container container)
    {
        public async Task<Result<CosmosResult<ImmutableArray<TEntity>>>> Query<TEntity>(
            CosmosQuerySpecification<TEntity> spec,
            CancellationToken cancellationToken = default)
            where TEntity : CosmosDocument
        {
            try
            {
                var options = BuildRequestOptions(spec);

                var queryable = container.GetItemLinqQueryable<TEntity>(requestOptions: options)
                    .ApplyPredicates(spec)
                    .ApplyOrdering(spec)
                    .ApplyMaxItems(spec);

                using var iterator = queryable.ToFeedIterator();
                var builder = ImmutableArray.CreateBuilder<TEntity>();
                var totalCharge = 0d;
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    builder.AddRange(response);
                    totalCharge += response.RequestCharge;
                }

                return new CosmosResult<ImmutableArray<TEntity>>(builder.ToImmutable(), totalCharge);
            }
            catch (CosmosException e)
            {
                return CosmosQueryError.Create(e, container.Id);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return ExceptionError.FromException(e);
            }
        }

        public async Task<Result<CosmosResult<CosmosQueryPage<TEntity>>>> QueryPage<TEntity>(
            CosmosQuerySpecification<TEntity> spec,
            CancellationToken cancellationToken = default)
            where TEntity : CosmosDocument
        {
            try
            {
                var options = BuildRequestOptions(spec);

                var queryable = container
                    .GetItemLinqQueryable<TEntity>(
                        continuationToken: spec.ContinuationToken,
                        requestOptions: options)
                    .ApplyPredicates(spec)
                    .ApplyOrdering(spec);

                using var iterator = queryable.ToFeedIterator();
                var response = await iterator.ReadNextAsync(cancellationToken);

                var page = new CosmosQueryPage<TEntity>(
                    [.. response],
                    response.ContinuationToken,
                    iterator.HasMoreResults);

                return new CosmosResult<CosmosQueryPage<TEntity>>(page, response.RequestCharge);
            }
            catch (CosmosException e)
            {
                return CosmosQueryError.Create(e, container.Id);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                return ExceptionError.FromException(e);
            }
        }
    }
}
