using System.Linq.Expressions;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface IAlternativeTitlesGetter
{
    Task<Either<DomainError, ImmutableList<AlternativeTitleStorage>>>
        GetForSeason(string? key, CancellationToken token);

    Task<Either<DomainError, ImmutableList<TilesMap>>> ByOriginalTitles(IEnumerable<string> originalTitles,
        CancellationToken token);

    Task<Either<DomainError, AlternativeTitleStorage>> GetSingle(RowKey rowKey, PartitionKey partitionKey,
        CancellationToken token);
}

public sealed class AlternativeTitlesGetter : IAlternativeTitlesGetter
{
    private readonly ITableClientFactory<AlternativeTitleStorage> _tableClientFactory;

    public AlternativeTitlesGetter(ITableClientFactory<AlternativeTitleStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<AlternativeTitleStorage>>> GetForSeason(string? key,
        CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<AlternativeTitleStorage>(t => t.PartitionKey == key,
                    cancellationToken: token)));
    }

    public Task<Either<DomainError, ImmutableList<TilesMap>>> ByOriginalTitles(IEnumerable<string> originalTitles,
        CancellationToken token)
    {
        // TODO: refactor to add status to the alternatiive tile
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<AlternativeTitleStorage>(x => x.Status != SeriesStatus.Completed,
                    cancellationToken: token)))
            .MapAsync(items => items.Where(i => originalTitles.Contains(i.OriginalTitle)).ToImmutableList())
            .MapAsync(items =>
                items.ConvertAll(i =>
                    new TilesMap(i.OriginalTitle ?? string.Empty, i.AlternativeTitle ?? string.Empty)));
    }

    public Task<Either<DomainError, AlternativeTitleStorage>> GetSingle(RowKey rowKey, PartitionKey partitionKey,
        CancellationToken token)
    {
       return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQuery(() => client.QueryAsync<AlternativeTitleStorage>(x =>
                    x.RowKey == rowKey && x.PartitionKey == partitionKey, cancellationToken: token)))
            .MapAsync(results => results.First());
    }

    private static Expression<Func<AlternativeTitleStorage, bool>> GetTitlesFilter(IEnumerable<string> originalTitles)
    {
        var param = Expression.Parameter(typeof(AlternativeTitleStorage), "x");
        Expression body = Expression.Constant(false);
        body = originalTitles.Aggregate(body,
            (current, title) => Expression.OrElse(current,
                Expression.Equal(
                    Expression.Property(param, typeof(AlternativeTitleStorage),
                        nameof(AlternativeTitleStorage.OriginalTitle)),
                    Expression.Constant(title))));

        return Expression.Lambda<Func<AlternativeTitleStorage, bool>>(body, param);
    }
}