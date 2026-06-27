namespace AnimeFeedManager.Infrastructure.Cosmos.Results.Static;

public static class CosmosResultExtensions
{
    extension<TOrigin>(CosmosResult<ImmutableArray<TOrigin>> result)
    {
        /// <summary>
        /// Projects each element of the wrapped <see cref="ImmutableArray{TOrigin}"/> using
        /// <paramref name="mapper"/>, preserving the original <see cref="CosmosResult{T}.RequestCharge"/>.
        /// </summary>
        public CosmosResult<ImmutableArray<TTarget>> MapItems<TTarget>(Func<TOrigin, TTarget> mapper)
        {
            var items = result.Value;

            if (items.IsEmpty)
                return new(ImmutableArray<TTarget>.Empty, result.RequestCharge);

            var builder = ImmutableArray.CreateBuilder<TTarget>(items.Length);
            foreach (var item in items)
                builder.Add(mapper(item));

            return new(builder.ToImmutable(), result.RequestCharge);
        }
    }
}
