using System.Text.Json.Serialization.Metadata;

namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record FeedTitlesUpdateResult(SeriesSeason Season, int UpdatedFeed) : SerializableEventPayload<FeedTitlesUpdateResult>
{
    public override JsonTypeInfo<FeedTitlesUpdateResult> GetJsonTypeInfo() => FeedTitlesUpdateResultContext.Default.FeedTitlesUpdateResult;
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(FeedTitlesUpdateResult))]
[EventPayloadSerializerContext(typeof(FeedTitlesUpdateResult))]
public partial class FeedTitlesUpdateResultContext : JsonSerializerContext;

public sealed record FeedTitlesUpdateError(SeriesSeason Season) : SerializableEventPayload<FeedTitlesUpdateError>
{
    public override JsonTypeInfo<FeedTitlesUpdateError> GetJsonTypeInfo() => FeedTitlesUpdateErrorContext.Default.FeedTitlesUpdateError;
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(FeedTitlesUpdateError))]
[EventPayloadSerializerContext(typeof(FeedTitlesUpdateError))]
public partial class FeedTitlesUpdateErrorContext : JsonSerializerContext;