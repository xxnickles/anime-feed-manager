using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Seasons.Types;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ISeasonsGetter
{
    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons(CancellationToken token);

    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetLastFourSeasons(CancellationToken token);
    
    Task<Either<DomainError, SeasonStorage>> GetCurrentSeason(CancellationToken token);
}

public sealed class SeasonsGetter(ITableClientFactory<SeasonStorage> tableClientFactory) : ISeasonsGetter
{
    public Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() => client.QueryAsync<SeasonStorage>(season =>
                    season.PartitionKey == SeasonType.Season || season.PartitionKey == SeasonType.Latest,
                cancellationToken: token)));
    }

    public Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetLastFourSeasons(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteLimitedQuery(
                () => client.QueryAsync<SeasonStorage>(season =>
                        season.PartitionKey == SeasonType.Season || season.PartitionKey == SeasonType.Latest, 4,
                    cancellationToken: token), 4));
    }

    public Task<Either<DomainError, SeasonStorage>> GetCurrentSeason(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteLimitedQuery(
                () => client.QueryAsync<SeasonStorage>(season =>
                        season.PartitionKey == SeasonType.Season || season.PartitionKey == SeasonType.Latest, 1,
                    cancellationToken: token), 1))
            .MapAsync(items => items.First());
    }
}