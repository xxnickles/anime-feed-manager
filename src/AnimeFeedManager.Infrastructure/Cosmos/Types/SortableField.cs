using System.Text.Json.Serialization;

namespace AnimeFeedManager.Infrastructure.Cosmos.Types;

/// <summary>
/// Holds a pre-built ordering delegate for a single sortable field. The For factory
/// captures a typed Expression&lt;Func&lt;TEntity, TKey&gt;&gt; in a closure at creation time,
/// avoiding type erasure (LambdaExpression) and expression tree reconstruction at
/// query time. The trade-off is that the delegate is opaque — the original expression
/// is not inspectable — but nothing in the query pipeline requires inspection.
/// </summary>
public sealed record SortableField<TEntity>
{
    internal Func<IQueryable<TEntity>, SortDirection, IOrderedQueryable<TEntity>> Apply { get; }

    internal SortableField(
        Func<IQueryable<TEntity>, SortDirection, IOrderedQueryable<TEntity>> apply)
        => Apply = apply;
}

[JsonConverter(typeof(JsonStringEnumConverter<SortDirection>))]
public enum SortDirection { Ascending, Descending }
