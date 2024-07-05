using AnimeFeedManager.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ISeasonsGetter
{
    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons(CancellationToken token);

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


    public Task<Either<DomainError, SeasonStorage>> GetCurrentSeason(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteLimitedQuery(
                () => client.QueryAsync<SeasonStorage>(season =>
                        season.PartitionKey == SeasonType.Season || season.PartitionKey == SeasonType.Latest, 1,
                    cancellationToken: token)))
            .BindAsync(GetLatest);
    }

    private static Either<DomainError, SeasonStorage> GetLatest(ImmutableList<SeasonStorage> availableSeasons)
    {
        return availableSeasons.Count > 0
            ? availableSeasons.First()
            : NotFoundError.Create("No season information has been found");
    }
}