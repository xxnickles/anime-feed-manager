using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Types;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Series.IO;

public interface IAlternativeTitlesStore
{
    Task<Either<DomainError, Unit>> Upsert(AlternativeTitleStorage series, CancellationToken token);
}

public sealed class AlternativeTitlesStore : IAlternativeTitlesStore
{
    private readonly ITableClientFactory<AlternativeTitleStorage> _tableClientFactory;

    public AlternativeTitlesStore(ITableClientFactory<AlternativeTitleStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }
    public Task<Either<DomainError, Unit>> Upsert(AlternativeTitleStorage series, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(
                client => TableUtils.TryExecute(() => client.UpsertEntityAsync(series, cancellationToken: token)))
            .MapAsync(_ => unit);
    }
}