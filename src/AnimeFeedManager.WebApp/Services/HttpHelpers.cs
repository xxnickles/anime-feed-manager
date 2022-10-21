using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AnimeFeedManager.WebApp.Exceptions;

namespace AnimeFeedManager.WebApp.Services;

public static class HttpHelpers
{
    private record ProblemDetails(string Type, string Title, string Instance, HttpStatusCode Status,
        ImmutableDictionary<string, string[]>? Errors, string? Detail);
    
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

    /// <summary>
    /// Wraps default EnsureSuccessStatusCode with custom problem entity management. It read the body; therefore there is always a penalty
    /// WARNING! Use only on responses that returns problem details!
    /// </summary>
    /// <returns></returns>
    public static async Task CheckForProblemDetails(this HttpResponseMessage response)
    {
        Console.WriteLine($"error response? {response.StatusCode >= HttpStatusCode.BadRequest}");
        // Only check for errors!
        if (response.StatusCode >= HttpStatusCode.BadRequest)
        {
            var bodyStream = await response.Content.ReadAsStreamAsync();
            var bodyObject = await JsonSerializer.DeserializeAsync<ProblemDetails>(bodyStream);
            if (bodyObject?.Errors != null)
            {
                Console.WriteLine($"has errors {bodyObject.Errors}");
                throw new HttpProblemDetailsValidationException(bodyObject.Title, bodyObject.Errors,
                    response.StatusCode);
            }

            if (!string.IsNullOrEmpty(bodyObject?.Detail))
            {
                Console.WriteLine($"has detail {bodyObject.Detail}");
                throw new HttpProblemDetailsException(bodyObject.Title, bodyObject.Detail, response.StatusCode);
            }
              

        }
        
        // Default behavior
        response.EnsureSuccessStatusCode();
    } 
}