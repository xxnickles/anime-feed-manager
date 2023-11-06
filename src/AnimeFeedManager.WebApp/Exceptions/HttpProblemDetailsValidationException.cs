using System.Collections.Immutable;
using System.Net;

namespace AnimeFeedManager.WebApp.Exceptions;

public class HttpProblemDetailsValidationException(
    string message,
    ImmutableDictionary<string, string[]> errors,
    HttpStatusCode statusCode)
    : HttpRequestException(message, null, statusCode)
{
    public ImmutableDictionary<string, string[]> Errors { get; } = errors;
}