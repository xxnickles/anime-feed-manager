using System.Text.Json;

namespace AnimeFeedManager.Functions;

public static class Serializer
{
    public static T? FromJson<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T>(jsonString,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }

    public static ValueTask<T?> FromJson<T>(Stream stream)
    {
        return JsonSerializer.DeserializeAsync<T>(stream,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }

    public static string ToJson<T>(T data)
    {
        return JsonSerializer.Serialize(data,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }
}