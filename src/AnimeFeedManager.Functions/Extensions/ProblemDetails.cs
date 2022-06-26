using System.Collections.Immutable;
using System.Net;

namespace AnimeFeedManager.Functions.Extensions;

public abstract record ProblemDetails(string Type, string Title, string Instance, HttpStatusCode Status);

public record ErrorProblemDetails
    (string Type, string Title, string Instance, HttpStatusCode Status, string Detail) : ProblemDetails(Type, Title,
        Instance, Status);

public record ValidationProblemDetails(string Type, string Title, string Instance, HttpStatusCode Status,
    ImmutableDictionary<string, string[]> Errors) : ProblemDetails(Type, Title, Instance, Status);


public record NotFoundProblemDetail(string Instance, string Detail)
    : ErrorProblemDetails(
        "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/404",
        "Not Found",
        Instance,
        HttpStatusCode.NotFound,
        Detail);
public record EmptyUnprocessableEntityProblemDetail(string Instance) :
    ErrorProblemDetails(
        "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422",
        "One or more validation errors occurred.",
        Instance,
        HttpStatusCode.UnprocessableEntity,
        "Validation Failed");

public record UnprocessableEntityProblemDetail(string Instance, ImmutableDictionary<string, string[]> Errors) :
    ValidationProblemDetails(
        "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422",
        "One or more validation errors occurred.",
        Instance,
        HttpStatusCode.UnprocessableEntity,
        Errors);


public record InternalErrorEntityProblemDetail(string Instance, string Detail) :
    ErrorProblemDetails(
        "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500",
        "An error occurred when processing your request.",
        Instance,
        HttpStatusCode.InternalServerError,
        Detail);
