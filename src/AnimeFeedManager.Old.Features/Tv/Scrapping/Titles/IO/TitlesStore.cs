using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Scrapping.Titles.Types;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Titles.IO;

public interface ITitlesStore
{
    public Task<Either<DomainError, Unit>> UpdateTitles(IEnumerable<string> titles, CancellationToken token);
}

public sealed class TitlesStore(ITableClientFactory<TitlesStorage> tableClientFactory) : ITitlesStore
{
    public Task<Either<DomainError, Unit>> UpdateTitles(IEnumerable<string> titles, CancellationToken token )
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.TryExecute(() => client.UpsertEntityAsync(GetEntity(titles), cancellationToken: token)))
            .MapAsync(_ => unit);
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