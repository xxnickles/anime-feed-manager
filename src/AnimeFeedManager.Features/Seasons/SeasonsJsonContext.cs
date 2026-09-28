using AnimeFeedManager.Features.Seasons.Events;

namespace AnimeFeedManager.Features.Seasons;

// Domain Messages
[JsonSerializable(typeof(SeasonUpdated))]
// Event Payloads
[JsonSerializable(typeof(SeasonUpdateResult))]
[EventPayloadSerializerContext(typeof(SeasonUpdateResult))]
[JsonSerializable(typeof(NewSeasonAdded))]
[EventPayloadSerializerContext(typeof(NewSeasonAdded))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class SeasonsJsonContext : JsonSerializerContext;
