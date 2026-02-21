using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.Tv;

// Domain Messages
[JsonSerializable(typeof(FeedTitlesUpdated))]
[JsonSerializable(typeof(SeriesFeedUpdated))]
[JsonSerializable(typeof(CompletedSeries))]
[JsonSerializable(typeof(CompleteOngoingSeries))]
[JsonSerializable(typeof(UpdatedToOngoing))]
[JsonSerializable(typeof(UpdateTvSeriesEvent))]
[JsonSerializable(typeof(UpdateLatestFeedTitlesEvent))]
[JsonSerializable(typeof(FeedNotification))]
[JsonSerializable(typeof(RunFeedNotification))]
// Event Payloads
[JsonSerializable(typeof(ScrapTvLibraryResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryResult))]
[JsonSerializable(typeof(ScrapTvLibraryFailedResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryFailedResult))]
[JsonSerializable(typeof(FeedTitlesUpdateResult))]
[EventPayloadSerializerContext(typeof(FeedTitlesUpdateResult))]
[JsonSerializable(typeof(FeedTitlesUpdateError))]
[EventPayloadSerializerContext(typeof(FeedTitlesUpdateError))]
[JsonSerializable(typeof(CompletedTvSeriesResult))]
[EventPayloadSerializerContext(typeof(CompletedTvSeriesResult))]
[JsonSerializable(typeof(AutoSubscriptionResult))]
[EventPayloadSerializerContext(typeof(AutoSubscriptionResult))]
[JsonSerializable(typeof(NotificationSent))]
[EventPayloadSerializerContext(typeof(NotificationSent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class TvJsonContext : JsonSerializerContext;
