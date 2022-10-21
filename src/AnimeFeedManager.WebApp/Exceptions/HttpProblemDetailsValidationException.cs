using System.Collections.Immutable;
using System.Net;

namespace AnimeFeedManager.WebApp.Exceptions;

public class HttpProblemDetailsValidationException : HttpRequestException
{
    public ImmutableDictionary<string, string[]> Errors { get; }
    public HttpProblemDetailsValidationException(string message, ImmutableDictionary<string, string[]> errors, HttpStatusCode statusCode) : base(message,null, statusCode)
    {
        Errors = errors;
    }
}