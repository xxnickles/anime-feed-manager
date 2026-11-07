using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.Tv;

// Domain Messages
[JsonSerializable(typeof(UpdatedToOngoing))]
[JsonSerializable(typeof(UpdateTvSeriesEvent))]
[JsonSerializable(typeof(FeedNotification))]
[JsonSerializable(typeof(RunFeedNotification))]
[JsonSerializable(typeof(NyaaFeedCompleted))]
[JsonSerializable(typeof(BindNyaaSeries))]
[JsonSerializable(typeof(BackfillNyaaSeries))]
[JsonSerializable(typeof(RunAutomatedBinding))]
[JsonSerializable(typeof(RunFeedSettlement))]
// Event Payloads
[JsonSerializable(typeof(ScrapTvLibraryResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryResult))]
[JsonSerializable(typeof(ScrapTvLibraryFailedResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryFailedResult))]
[JsonSerializable(typeof(AutoSubscriptionResult))]
[EventPayloadSerializerContext(typeof(AutoSubscriptionResult))]
[JsonSerializable(typeof(NotificationSent))]
[EventPayloadSerializerContext(typeof(NotificationSent))]
[JsonSerializable(typeof(NyaaSeriesBound))]
[EventPayloadSerializerContext(typeof(NyaaSeriesBound))]
[JsonSerializable(typeof(NyaaBindFailed))]
[EventPayloadSerializerContext(typeof(NyaaBindFailed))]
[JsonSerializable(typeof(AutomatedBindingCompleted))]
[EventPayloadSerializerContext(typeof(AutomatedBindingCompleted))]
[JsonSerializable(typeof(AutomatedBindingFailed))]
[EventPayloadSerializerContext(typeof(AutomatedBindingFailed))]
[JsonSerializable(typeof(NyaaBackfillFailed))]
[EventPayloadSerializerContext(typeof(NyaaBackfillFailed))]
// Stored records
[JsonSerializable(typeof(NyaaPoll))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class TvJsonContext : JsonSerializerContext;
