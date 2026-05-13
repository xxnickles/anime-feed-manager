using System.Linq.Expressions;
using AnimeFeedManager.Infrastructure.Cosmos.Static;

namespace AnimeFeedManager.Infrastructure.Cosmos.Types;

public class CosmosQuerySpecificationSortBuilder<TEntity> where TEntity : CosmosDocument
{
    private readonly List<KeyValuePair<string, SortableField<TEntity>>> _fields = [];

    public CosmosQuerySpecificationSortBuilder<TEntity> Field<TKey>(
        string name,
        Expression<Func<TEntity, TKey>> keySelector)
    {
        _fields.Add(new(name, SortableField<TEntity>.For(keySelector)));
        return this;
    }

    public IReadOnlyList<KeyValuePair<string, SortableField<TEntity>>> Build() => _fields;
}
