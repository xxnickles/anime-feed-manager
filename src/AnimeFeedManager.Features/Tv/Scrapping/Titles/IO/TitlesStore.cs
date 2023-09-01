using AnimeFeedManager.Features.Tv.Scrapping.Titles.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;

public interface ITitlesStore
{
    public Task<Either<DomainError, Unit>> UpdateTitles(IEnumerable<string> titles, CancellationToken token);
}

public sealed class TitlesStore : ITitlesStore
{
    private readonly ITableClientFactory<TitlesStorage> _tableClientFactory;

    public TitlesStore(ITableClientFactory<TitlesStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }
    
    public Task<Either<DomainError, Unit>> UpdateTitles(IEnumerable<string> titles, CancellationToken token )
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.TryExecute(() => client.UpsertEntityAsync(GetEntity(titles), cancellationToken: token)))
            .MapAsync(x => unit);
    }

    private static TitlesStorage GetEntity(IEnumerable<string> titles)
    {
        return new TitlesStorage
        {
            Titles = string.Join(',', Utils.ReplaceTitleCommas(titles)), 
            PartitionKey = Utils.TitlesPartitionKey,
            RowKey = Utils.RowKey
        };
    }
}