using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Infrastructure.Cosmos.Types;

/// <summary>
/// Abstract base for Cosmos DB query specifications. Each concrete specification
/// declares predicates (filtering), sortable fields, and accepts client-driven
/// sorting, max-items, and continuation token parameters via model binding.
/// </summary>
/// <param name="PartitionKey">
/// When set, scopes the query to a single logical partition (most efficient).
/// When null, the query fans out across all partitions (cross-partition).
/// </param>
/// <param name="MaxItems">
/// For single-shot <c>Query</c>: total cap on items returned (translated to SQL <c>TOP N</c>).
/// For paginated <c>QueryPage</c>: maximum items per page (paired with <see cref="ContinuationToken"/>).
/// Defaults to <c>1000</c> as a safety bound against accidental full-collection scans.
/// Pass <c>-1</c> to explicitly opt into unbounded / SDK default.
/// </param>
public abstract record CosmosQuerySpecification<TEntity>(
    PartitionKey? PartitionKey,
    string? SortBy,
    SortDirection? SortDir,
    int MaxItems = 1000,
    string? ContinuationToken = null) where TEntity : CosmosDocument
{
    /// <summary>
    /// Filter expressions applied as WHERE clauses. Each expression is ANDed together.
    /// Concrete specifications yield one predicate per non-null query parameter.
    /// </summary>
    public virtual IEnumerable<Expression<Func<TEntity, bool>>> Predicates() => [];

    /// <summary>
    /// Sortable field declarations mapping client-facing query parameter names to
    /// document property expressions. Keys are matched case-insensitively against
    /// <see cref="SortBy"/> by the infrastructure — concrete types only provide the mappings.
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, SortableField<TEntity>>> SortableFields => [];
}
