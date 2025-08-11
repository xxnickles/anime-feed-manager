using AnimeFeedManager.Features.SystemEvents.Storage;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.SystemEvents.UpdateProcess;

public static class SystemEventUpdate
{
    public static SystemEventUpdateData StartProcess(SystemEvent systemEvent) => systemEvent.Target switch
    {
        EventTarget.LocalStorage => new LocalStorage(systemEvent, CreateEventStorage(systemEvent)),
        EventTarget.Browser => new Notification(systemEvent),
        EventTarget.Both => new FullUpdate(systemEvent, CreateEventStorage(systemEvent)),
        _ => throw new ArgumentOutOfRangeException(nameof(systemEvent))
    };

    public static Task<Result<SystemEventUpdateData>> StoreEvent(this SystemEventUpdateData data,
        EventUpdater eventUpdater,
        CancellationToken cancellationToken)
    {
        return (data switch
        {
            FullUpdate fullUpdate => eventUpdater(fullUpdate.Storage, cancellationToken),
            LocalStorage localStorage => eventUpdater(localStorage.Storage, cancellationToken),
            _ => Task.FromResult(Result<Unit>.Success())
        }).Map(_ => data);
    }

    public static Task<Result<UpdateResult>> PrepareUiNotification(this Task<Result<SystemEventUpdateData>> data)
    {
        return data.Map(d => d switch
        {
            LocalStorage localStorage => new UpdateResult(localStorage.Event, false),
            _ => new UpdateResult(d.Event, true)
        });
    }

    private static EventStorage CreateEventStorage(SystemEvent systemEvent) => new()
    {
        RowKey = IdHelpers.GetUniqueId(),
        PartitionKey = systemEvent.Consumer,
        EventType = systemEvent.Type,
        PayloadData = systemEvent.Payload.Payload,
        PayloadTypeName = systemEvent.Payload.PayloadTypeName,
        Target = systemEvent.Target,
    };
}