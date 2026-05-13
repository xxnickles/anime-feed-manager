using System.Linq.Expressions;
using AnimeFeedManager.Infrastructure.Cosmos.Types;

namespace AnimeFeedManager.Infrastructure.Cosmos.Static;

public static class SortableFieldExtensionMembers
{
    extension<TEntity>(SortableField<TEntity>)
    {
        public static SortableField<TEntity> For<TKey>(
            Expression<Func<TEntity, TKey>> keySelector) =>
            new((query, direction) => direction == SortDirection.Ascending
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector));
    }
}
