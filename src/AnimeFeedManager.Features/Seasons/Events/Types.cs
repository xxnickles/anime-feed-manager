using System.Text.Json.Serialization.Metadata;

namespace AnimeFeedManager.Features.Seasons.Events;

public enum SeasonUpdateStatus
{
    New,
    Updated,
    NoChanges,
    Error
}

public sealed record SeasonUpdateResult(SeriesSeason Season, SeasonUpdateStatus SeasonUpdateStatus)
    : SerializableEventPayload<SeasonUpdateResult>
{
    public override JsonTypeInfo<SeasonUpdateResult> GetJsonTypeInfo()
    {
       return SeasonUpdatedResultContext.Default.SeasonUpdateResult;
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SeasonUpdateResult))]
[EventPayloadSerializerContext(typeof(SeasonUpdateResult))]
public partial class SeasonUpdatedResultContext : JsonSerializerContext;
