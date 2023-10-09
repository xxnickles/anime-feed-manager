using System.Net;

namespace AnimeFeedManager.WebApp.Exceptions
{
    public class HttpProblemDetailsException : HttpRequestException
    {
        public string Detail { get; }
        public HttpProblemDetailsException(string message, string detail, HttpStatusCode statusCode) : base(message,null, statusCode)
        {
            Detail = detail;
        }
    }
}