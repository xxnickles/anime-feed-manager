using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.State.IO;
using AnimeFeedManager.Features.State.Types;
using AnimeFeedManager.Storage.Domain;
using AnimeInfoStorage = AnimeFeedManager.Features.Tv.Scrapping.Series.Types.Storage.AnimeInfoStorage;

namespace AnimeFeedManager.Features.Images.IO;

public class TvImageStorage : ITvImageStorage
{
    private readonly IStateUpdater _stateUpdaterUpdater;
    private readonly ITableClientFactory<AnimeInfoStorage> _tableClientFactory;

    public TvImageStorage(
        IStateUpdater stateUpdaterUpdater,
        ITableClientFactory<AnimeInfoStorage> tableClientFactory)
    {
        _stateUpdaterUpdater = stateUpdaterUpdater;
        _tableClientFactory = tableClientFactory;
    }

    public async Task<Either<DomainError, Unit>> AddTvImage(StateWrap<DownloadImageEvent> imageStateWrap,
        string imageUrl, CancellationToken token)
    {
        var storeResult = await _tableClientFactory.GetClient()
            .BindAsync(client => Store(client, imageUrl, imageStateWrap, token));

        return await _stateUpdaterUpdater.Update(storeResult,
            new ImageStateChange(imageStateWrap.StateId, StateUpdateTarget.Images, SeriesType.Tv), token);
    }

    private static Task<Either<DomainError, Unit>> Store(TableClient client,
        string imageUrl,
        StateWrap<DownloadImageEvent> stateWrap,
        CancellationToken token)
    {
        var storageEntity = new ImageStorage
        {
            ImageUrl = imageUrl,
            PartitionKey = stateWrap.Payload.Partition,
            RowKey = stateWrap.Payload.Id
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storageEntity, cancellationToken: token),
                nameof(ImageStorage))
            .MapAsync(_ => unit);
    }
}