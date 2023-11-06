using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.IO;

public interface IOvasStorage
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<OvaStorage> series, CancellationToken token);
}

public sealed class OvasStorage(ITableClientFactory<OvaStorage> tableClientFactory) : IOvasStorage
{
    public Task<Either<DomainError, Unit>> Add(ImmutableList<OvaStorage> series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series,  token))
            .MapAsync(_ => unit);
    }
}