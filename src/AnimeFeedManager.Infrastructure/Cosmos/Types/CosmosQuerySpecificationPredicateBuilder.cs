using System.Linq.Expressions;

namespace AnimeFeedManager.Infrastructure.Cosmos.Types;

public class CosmosQuerySpecificationPredicateBuilder<TEntity> where TEntity : CosmosDocument
{
    private readonly List<Expression<Func<TEntity, bool>>> _predicates = [];

    public CosmosQuerySpecificationPredicateBuilder<TEntity> When<TValue>(
        TValue? value,
        Func<TValue, Expression<Func<TEntity, bool>>> factory)
        where TValue : struct
    {
        if (value.HasValue)
            _predicates.Add(factory(value.Value));
        return this;
    }

    public CosmosQuerySpecificationPredicateBuilder<TEntity> When<TValue>(
        TValue? value,
        Func<TValue, Expression<Func<TEntity, bool>>> factory)
        where TValue : class
    {
        if (value is not null)
            _predicates.Add(factory(value));
        return this;
    }

    public CosmosQuerySpecificationPredicateBuilder<TEntity> When<TValue>(
        TValue? value,
        Func<TValue, bool> guard,
        Func<TValue, Expression<Func<TEntity, bool>>> factory)
        where TValue : struct
    {
        if (value is { } v && guard(v))
            _predicates.Add(factory(v));
        return this;
    }

    public CosmosQuerySpecificationPredicateBuilder<TEntity> WhenAny<TItem>(
        IReadOnlyList<TItem>? items,
        Func<IEnumerable<TItem>, Expression<Func<TEntity, bool>>> factory)
    {
        if (items is { Count: > 0 })
            _predicates.Add(factory(items));
        return this;
    }

    public CosmosQuerySpecificationPredicateBuilder<TEntity> WhenByteEnum<TEnum>(
        TEnum[]? values,
        TEnum sentinel,
        Func<List<byte>, Expression<Func<TEntity, bool>>> factory)
        where TEnum : struct, Enum
    {
        var sentinelByte = Convert.ToByte(sentinel);
        var filtered = values?
            .Select(v => Convert.ToByte(v))
            .Where(b => b != sentinelByte)
            .ToList();

        if (filtered is not { Count: > 0 }) return this;

        _predicates.Add(factory(filtered));
        return this;
    }

    public IReadOnlyList<Expression<Func<TEntity, bool>>> Build() => _predicates;
}
