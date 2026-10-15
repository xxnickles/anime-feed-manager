namespace AnimeFeedManager.Features.Tv.Library.Storage;

/// <summary>Stored shape of one streaming link; <paramref name="Url"/> has no scheme.</summary>
internal sealed record StoredStream(
    string? Platform,
    string? Name,
    string? Url);

[JsonSerializable(typeof(StoredStream[]))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class StoredStreamsJsonContext : JsonSerializerContext;

internal static class StoredStreams
{
    /// <summary>Null when there is nothing to store, so a merge keeps the previous value.</summary>
    internal static string? ToStoredString(StoredStream[]? streams) =>
        streams is null or []
            ? null
            : JsonSerializer.Serialize(streams, StoredStreamsJsonContext.Default.StoredStreamArray);
}
