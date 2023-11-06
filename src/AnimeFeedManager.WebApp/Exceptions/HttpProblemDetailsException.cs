using System.Net;

namespace AnimeFeedManager.WebApp.Exceptions;

public class HttpProblemDetailsException(string message, string detail, HttpStatusCode statusCode)
    : HttpRequestException(message, null, statusCode)
{
    public string Detail { get; } = detail;
}