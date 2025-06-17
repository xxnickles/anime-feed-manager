using AnimeFeedManager.Features.SystemEvents.Storage;

namespace AnimeFeedManager.Features.SystemEvents.UpdateProcess;

public abstract record SystemEventUpdateData(SystemEvent Event);

public sealed record FullUpdate(SystemEvent Event, EventStorage Storage) : SystemEventUpdateData(Event);

public sealed record LocalStorage(SystemEvent Event, EventStorage Storage) : SystemEventUpdateData(Event);

public sealed record Notification(SystemEvent Event) : SystemEventUpdateData(Event);

public sealed record UpdateResult(SystemEvent Event, bool NotifyUi);