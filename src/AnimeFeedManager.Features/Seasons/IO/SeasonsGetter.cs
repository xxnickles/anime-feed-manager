using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Seasons.Types;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ISeasonsGetter
{
    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons(CancellationToken token);
}

public sealed class SeasonsGetter : ISeasonsGetter
{
    private readonly ITableClientFactory<SeasonStorage> _tableClientFactory;

    public SeasonsGetter(ITableClientFactory<SeasonStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }
    
    public Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons(CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() => client.QueryAsync<SeasonStorage>(season =>
                season.PartitionKey == SeasonType.Season || season.PartitionKey == SeasonType.Latest, cancellationToken: token)));
    }
}