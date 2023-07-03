using AnimeFeedManager.Features.Seasons.Types;

namespace AnimeFeedManager.Features.Seasons.IO;

public class SeasonStore : ISeasonStore
{
    private readonly ITableClientFactory<SeasonStorage> _tableClientFactory;

    public SeasonStore(ITableClientFactory<SeasonStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }
    
    public Task<Either<DomainError, Unit>> AddSeason(SeasonStorage season, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(
                client => TableUtils.TryExecute(() => client.UpsertEntityAsync(season, cancellationToken: token),
                nameof(SeasonStorage)))
            .MapAsync(_ => unit);
    }
}