using AnimeFeedManager.Features.Feeds.Entities;

namespace AnimeFeedManager.Features.Feeds;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> exposing the Feeds feature's
/// serializable types. Register alongside other feature contexts when wiring the Cosmos
/// serializer.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(FeedsDocument))]
[JsonSerializable(typeof(SeriesClassification))]
[JsonSerializable(typeof(NyaaConfirmation))]
[JsonSerializable(typeof(AiringClockFlag))]
[JsonSerializable(typeof(CollectionCheckpoint))]
[JsonSerializable(typeof(CollectionRun))]
[JsonSerializable(typeof(ReleaseDetected))]
[JsonSerializable(typeof(FeedsOccurrence))]
public partial class FeedsJsonContext : JsonSerializerContext;
