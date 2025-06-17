using System.Text.Json.Serialization.Metadata;

namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record ScrapTvLibraryResult(SeriesSeason Season, int UpdatedSeries, int NewSeries)
    : SerializableEventPayload<ScrapTvLibraryResult>
{
    public override JsonTypeInfo<ScrapTvLibraryResult> GetJsonTypeInfo() =>
        ScrapTvLibraryResultContext.Default.ScrapTvLibraryResult;
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ScrapTvLibraryResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryResult))]
public partial class ScrapTvLibraryResultContext : JsonSerializerContext;

public sealed record ScrapTvLibraryFailedResult(string Season) : SerializableEventPayload<ScrapTvLibraryFailedResult>
{
    public override JsonTypeInfo<ScrapTvLibraryFailedResult> GetJsonTypeInfo() =>
        ScrapTvLibraryFailedResultContext.Default.ScrapTvLibraryFailedResult;
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ScrapTvLibraryFailedResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryFailedResult))]
public partial class ScrapTvLibraryFailedResultContext : JsonSerializerContext;

