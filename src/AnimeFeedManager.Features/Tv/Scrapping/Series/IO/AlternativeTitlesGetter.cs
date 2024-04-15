using System.Linq.Expressions;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface IAlternativeTitlesGetter
{
    Task<Either<DomainError, ImmutableList<AlternativeTitleStorage>>>
        GetForSeason(string? key, CancellationToken token);

    Task<Either<DomainError, ImmutableList<TilesMap>>> ByOriginalTitles(IEnumerable<string> originalTitles,
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
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync(GetTitlesFilter(originalTitles), cancellationToken: token)))
            .MapAsync(items =>
                items.ConvertAll(i =>
                    new TilesMap(i.OriginalTitle ?? string.Empty, i.AlternativeTitle ?? string.Empty)));
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