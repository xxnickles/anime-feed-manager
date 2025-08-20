using System.Text.Json.Serialization.Metadata;

namespace AnimeFeedManager.Features.Tv.Library.Events;

public enum UpdateType
{
    FullLibrary,
    Titles
}

public sealed record ScrapTvLibraryResult(SeriesSeason Season, int UpdatedSeries, int NewSeries, UpdateType UpdateType = UpdateType.FullLibrary)
    : SerializableEventPayload<ScrapTvLibraryResult>
{
    public override JsonTypeInfo<ScrapTvLibraryResult> GetJsonTypeInfo() =>
        ScrapTvLibraryResultContext.Default.ScrapTvLibraryResult;
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ScrapTvLibraryResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryResult))]
public partial class ScrapTvLibraryResultContext : JsonSerializerContext;

public sealed record ScrapTvLibraryFailedResult(string Season, UpdateType UpdateType = UpdateType.FullLibrary) : SerializableEventPayload<ScrapTvLibraryFailedResult>
{
    public override JsonTypeInfo<ScrapTvLibraryFailedResult> GetJsonTypeInfo() =>
        ScrapTvLibraryFailedResultContext.Default.ScrapTvLibraryFailedResult;
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ScrapTvLibraryFailedResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryFailedResult))]
public partial class ScrapTvLibraryFailedResultContext : JsonSerializerContext;

