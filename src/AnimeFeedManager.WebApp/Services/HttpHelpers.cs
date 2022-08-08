using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;

namespace AnimeFeedManager.WebApp.Services;

public static class HttpHelpers
{
    public static async Task<ImmutableList<T>> MapToList<T>(this HttpResponseMessage response)
    {
        var result = await response.MapToObject(Enumerable.Empty<T>());
        return result.ToImmutableList();
    }
    
    public static async Task<T> MapToObject<T>(this HttpResponseMessage response, T defaultValue)
    {
        if(response.StatusCode == HttpStatusCode.NoContent)
            return defaultValue;
        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? defaultValue;
    }
    
}