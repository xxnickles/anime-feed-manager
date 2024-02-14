﻿using System.Text.Json;
using AnimeFeedManager.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ILatestSeasonStore
{
    Task<Either<DomainError, Unit>> Update(CancellationToken token);
}

public class LastedSeasonsStore : ILatestSeasonStore
{
    private readonly ISeasonsGetter _seasonsGetter;
    private readonly ITableClientFactory<LatestSeasonsStorage> _tableClientFactory;

    public LastedSeasonsStore(
        ISeasonsGetter seasonsGetter,
        ITableClientFactory<LatestSeasonsStorage> tableClientFactory)
    {
        _seasonsGetter = seasonsGetter;
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> Update(CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => Process(client, token));
    }

    private Task<Either<DomainError, Unit>> Process(TableClient client, CancellationToken token)
    {
        return _seasonsGetter.GetSeasons(10, token)
            .MapAsync(seasons =>
                seasons.ConvertAll(s => s.ToWrapper())
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.Season)
                    .Take(4)
                    .Reverse()
            )
            .BindAsync(items => Store(client, items, token));
    }

    private static Task<Either<DomainError, Unit>> Store(TableClient client,
        IEnumerable<SeasonWrapper> items,
        CancellationToken token)
    {
        return TableUtils.TryExecute(() => client.UpsertEntityAsync(new LatestSeasonsStorage
            {
                Payload = JsonSerializer.Serialize(items.Select(x => x.ToSimple()).ToArray(),
                    SimpleSeasonWrapperContext.Default.SimpleSeasonWrapperArray)
            }, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}