using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Features.Images.Types;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Old.Features.State.IO;
using AnimeFeedManager.Old.Features.State.Types;

namespace AnimeFeedManager.Old.Features.Images.IO;

public class MoviesImageStorage(
    IStateUpdater stateUpdaterUpdater,
    IDomainPostman domainPostman,
    ITableClientFactory<MovieStorage> tableClientFactory)
    : IMoviesImageStorage
{
    public async Task<Either<DomainError, Unit>> AddMoviesImage(StateWrap<DownloadImageEvent> imageStateWrap,
        string imageUrl, CancellationToken token)
    {
        var storeResult = await tableClientFactory.GetClient()
            .BindAsync(client => Store(client, imageUrl, imageStateWrap, token));

        return await stateUpdaterUpdater.Update(storeResult,
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
            SeriesType.Ova,
            $"Images for Movies have been scrapped. Completed: {currentState.Completed} Errors: {currentState.Errors}");
        
        return await domainPostman.SendMessage(notification, token);
    }
}