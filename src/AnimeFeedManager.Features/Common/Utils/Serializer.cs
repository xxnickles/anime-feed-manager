using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace AnimeFeedManager.Features.Common.Utils;

public static class Serializer
{
    [RequiresUnreferencedCode("Calls DynamicBehavior from Serializer.")]
    public static T? FromJson<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T>(jsonString,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }

    [RequiresUnreferencedCode("Calls DynamicBehavior from Serializer.")]
    public static ValueTask<T?> FromJson<T>(Stream stream)
    {
        return JsonSerializer.DeserializeAsync<T>(stream,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }

    [RequiresUnreferencedCode("Calls DynamicBehavior from Serializer.")]
    public static string ToJson<T>(T data)
    {
        return JsonSerializer.Serialize(data,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }
}