using System.Collections.Frozen;
using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ILatestSeasonsGetter
{
    Task<Either<DomainError, ImmutableList<SeasonWrapper>>> Get(CancellationToken token);
}

public class LatestSeasonsGetter : ILatestSeasonsGetter
{
    private readonly ITableClientFactory<LatestSeasonsStorage> _tableClientFactory;

    public LatestSeasonsGetter(ITableClientFactory<LatestSeasonsStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<SeasonWrapper>>> Get(CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() => client.QueryAsync<LatestSeasonsStorage>(season =>
                    season.PartitionKey == LatestSeasonsStorage.Partition && season.RowKey == LatestSeasonsStorage.Key,
                cancellationToken: token)))
            .MapAsync(items => items.First())
            .MapAsync(item => JsonSerializer.Deserialize(item.Payload ?? string.Empty,
                SimpleSeasonWrapperContext.Default.SimpleSeasonWrapperArray) ?? [])
            .MapAsync(items => items.Select(i => i.ToWrapper()).ToImmutableList());
    }
}