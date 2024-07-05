using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.State.IO;

public interface IStateCreator
{
    public Task<Either<DomainError, ImmutableList<StateWrap<T>>>> Create<T>(NotificationTarget target,
        ImmutableList<T> entities, Box messageBox) where T : DomainMessage;
}

public sealed class StateCreator(ITableClientFactory<StateUpdateStorage> tableClientFactory) : IStateCreator
{
    public Task<Either<DomainError, ImmutableList<StateWrap<T>>>> Create<T>(NotificationTarget target,
        ImmutableList<T> entities, Box messageBox) where T : DomainMessage
    {
        if (entities.IsEmpty)
            return Task.FromResult(
                Left<DomainError, ImmutableList<StateWrap<T>>>(
                    NotingToProcessError.Create("Collection of entities is empty")));

        var id = IdHelpers.GetUniqueId();
        var newState = new StateUpdateStorage
        {
            RowKey = id,
            PartitionKey = target.Value,
            Errors = 0,
            Completed = 0,
            ToUpdate = entities.Count
        };

        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.TryExecute(() => client.UpsertEntityAsync(newState)))
            .MapAsync(_ => entities.ConvertAll(e => new StateWrap<T>(id, e, messageBox)));
    }
}