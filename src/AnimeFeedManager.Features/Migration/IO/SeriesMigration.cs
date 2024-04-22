using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Migration.IO;

public class SeriesMigration(ITableClientFactory<LegacyAnimeInfoStorage> tableClientFactory)
{
    public Task<Either<DomainError, Unit>> MigrateTvSeries(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => MigrateCompleted(client, token))
            .BindAsync(client => MigrateIncomplete(client, token));
    }

    private Task<Either<DomainError, TableClient>> MigrateCompleted(TableClient client, CancellationToken token)
    {
        return TableUtils.ExecuteQuery(() => client.QueryAsync<LegacyAnimeInfoStorage>(a => a.Completed == true, cancellationToken: token))
            .MapAsync(items => items.ConvertAll(item =>
            {
                item.Status = SeriesStatus.Completed;
                return item;
            }))
            .BindAsync(items => BatchPerSeason(client, items,token))
            .MapAsync(_ => client);
    }

    private Task<Either<DomainError, Unit>> MigrateIncomplete(TableClient client, CancellationToken token)
    {
        return TableUtils.ExecuteQuery(() => client.QueryAsync<LegacyAnimeInfoStorage>(a => a.Completed == false))
            .MapAsync(items => items.ConvertAll(item =>
            {
                item.Status = !string.IsNullOrWhiteSpace(item.FeedTitle)
                    ? SeriesStatus.Ongoing
                    : SeriesStatus.NotAvailable;
                return item;
            }))
            .BindAsync(items => BatchPerSeason(client, items, token))
            .MapAsync(_ => unit);
    }

    private async Task<Either<DomainError, ImmutableList<Unit>>> BatchPerSeason(TableClient client,
        ImmutableList<LegacyAnimeInfoStorage> series, CancellationToken token)
    {
        var tasks = series.GroupBy(s => s.PartitionKey)
            .AsParallel()
            .Select(group => TableUtils.BatchAdd(client, group, token)).ToArray();
        var results = await Task.WhenAll(tasks);
       return results.FlattenResults();
    }
}