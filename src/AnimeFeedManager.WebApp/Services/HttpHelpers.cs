using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AnimeFeedManager.WebApp.Exceptions;

namespace AnimeFeedManager.WebApp.Services
{
    public static class HttpHelpers
    {
        private record ProblemDetails(string Type, string Title, string Instance, HttpStatusCode Status,
            ImmutableDictionary<string, string[]>? Errors, string? Detail);

        public static async Task<ImmutableList<string>> MapToListOfStrings(this HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
                return ImmutableList<string>.Empty;
            var result = await response.Content.ReadFromJsonAsync<string[]>();
            return result?.ToImmutableList() ?? ImmutableList<string>.Empty;
        }

        public static async Task<T> MapToObject<T>(this HttpResponseMessage response, T defaultValue)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
                return defaultValue;
            var result = await response.Content.ReadFromJsonAsync<T>();
            return result ?? defaultValue;
        }

        // TODO: Check why doesn't work
        //public static async Task<T> MapToObject<T>(this HttpResponseMessage response, JsonTypeInfo<T> jsonTypeInfo, T defaultValue)
        //{
        //    if (response.StatusCode == HttpStatusCode.NoContent)
        //        return defaultValue;
        //    var result = await response.Content.ReadFromJsonAsync(jsonTypeInfo);
        //    return result ?? defaultValue;
        //}
    
        public static async Task<string?> MapToString(this HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
                return default;
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        /// <summary>
        /// Wraps default EnsureSuccessStatusCode with custom problem entity management. It read the body; therefore there is always a penalty
        /// WARNING! Use only on responses that returns problem details!
        /// </summary>
        /// <returns></returns>
        public static async Task CheckForProblemDetails(this HttpResponseMessage response)
        {
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
}