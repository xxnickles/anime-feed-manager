using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Images.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.IO;
using AnimeInfoStorage = AnimeFeedManager.Features.Tv.Types.AnimeInfoStorage;

namespace AnimeFeedManager.Features.Images.IO;

public class TvImageStorage : ITvImageStorage
{
    private readonly IStateUpdater _stateUpdaterUpdater;
    private readonly IDomainPostman _domainPostman;
    private readonly ITableClientFactory<AnimeInfoStorage> _tableClientFactory;

    public TvImageStorage(
        IStateUpdater stateUpdaterUpdater,
        IDomainPostman domainPostman,
        ITableClientFactory<AnimeInfoStorage> tableClientFactory)
    {
        _stateUpdaterUpdater = stateUpdaterUpdater;
        _domainPostman = domainPostman;
        _tableClientFactory = tableClientFactory;
    }

    public async Task<Either<DomainError, Unit>> AddTvImage(StateWrap<DownloadImageEvent> imageStateWrap,
        string imageUrl, CancellationToken token)
    {
        var storeResult = await _tableClientFactory.GetClient()
            .BindAsync(client => Store(client, imageUrl, imageStateWrap, token));

        return await _stateUpdaterUpdater.Update(storeResult,
                new StateChange(imageStateWrap.StateId, NotificationTarget.Images, imageStateWrap.Payload.Id), token)
            .BindAsync(currentState => TryToPublishUpdate(currentState, token));
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

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storageEntity, cancellationToken: token))
            .MapAsync(_ => unit);
    }

    private async Task<Either<DomainError, Unit>> TryToPublishUpdate(CurrentState currentState, CancellationToken token)
    {
        if (!currentState.ShouldNotify) return unit;

        var notification = new ImageUpdateNotification(
            NotificationType.Information,
            SeriesType.Tv,
            $"Images for TV have been scrapped. Completed: {currentState.Completed} Errors: {currentState.Errors}");
        return await _domainPostman.SendMessage(notification, Box.ImageUpdateNotifications, token);
    }
}