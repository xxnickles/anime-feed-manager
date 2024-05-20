using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.State.IO;

public interface ICreateState
{
    public Task<Either<DomainError, ImmutableList<StateWrap<T>>>> Create<T>(NotificationTarget target,
        ImmutableList<T> entities, Box MessageBox) where T : DomainMessage;
}

public sealed class CreateState(ITableClientFactory<StateUpdateStorage> tableClientFactory) : ICreateState
{
    public Task<Either<DomainError, ImmutableList<StateWrap<T>>>> Create<T>(NotificationTarget target,
        ImmutableList<T> entities, Box MessageBox) where T : DomainMessage
    {
        if (!entities.Any())
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
            .MapAsync(_ => entities.ConvertAll(e => new StateWrap<T>(id, e, MessageBox)));
    }
}