using AnimeFeedManager.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ISeasonStore
{
    public Task<Either<DomainError, Unit>> AddSeason(SeasonStorage season, SeasonType seasonType,
        CancellationToken token);
}

public sealed class SeasonStore(ITableClientFactory<SeasonStorage> tableClientFactory) : ISeasonStore
{
    public Task<Either<DomainError, Unit>> AddSeason(SeasonStorage season, SeasonType seasonType,
        CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => CheckIfExist(client, season, token))
            .BindAsync(client => CleanLatest(client, seasonType, token))
            .BindAsync(
                client => TableUtils.TryExecute(() => client.UpsertEntityAsync(season, cancellationToken: token)))
            .MapAsync(_ => unit);
    }

    private static Task<Either<DomainError, TableClient>> CheckIfExist(TableClient client, SeasonStorage season,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<SeasonStorage>(s => s.Season == season.Season && s.Year == season.Year, cancellationToken: token))
            .BindAsync((Func<ImmutableList<SeasonStorage>, Either<DomainError, TableClient>>?) CreateResult);

        Either<DomainError, TableClient> CreateResult(ImmutableList<SeasonStorage> items) =>
            items.IsEmpty
                ? Right<DomainError, TableClient>(client)
                : SeasonExistError.Create(season);
    }

    private static Task<Either<DomainError, TableClient>> CleanLatest(TableClient client, SeasonType seasonType,
        CancellationToken token)
    {
        if (!seasonType.IsLatest()) return Task.FromResult(Right<DomainError, TableClient>(client));

        return TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<SeasonStorage>(s => s.PartitionKey == SeasonType.Latest))
            .BindAsync(i => RemoveLatest(client, i, token));
    }

    private static Task<Either<DomainError, TableClient>> RemoveLatest(TableClient client,
        ImmutableList<SeasonStorage> latestSeason,
        CancellationToken token)
    {
        if (latestSeason.IsEmpty) return Task.FromResult(Right<DomainError, TableClient>(client));

        return TableUtils.TryExecute(() =>
                client.DeleteEntityAsync(SeasonType.Latest, latestSeason.First().RowKey, cancellationToken: token))
            .MapAsync(_ => client);
    }
}