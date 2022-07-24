using System.Collections.Immutable;
using System.Net;
using MediatR;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Functions.Extensions;

public static class  HttpRequestDataExtensions
{

    public static ValueTask WriteAsProblemResponse<T>(this HttpResponseData response, T detail, CancellationToken cancellationToken = default) where T : ProblemDetails
    {
        return response.WriteAsJsonAsync(detail, "application/problem+json", detail.Status, cancellationToken);
    }

    private static async Task<HttpResponseData> CreateErrorDetailResponse(this HttpRequestData request,
        ProblemDetails problem)
    {
        var response = request.CreateResponse();
        await response.WriteAsProblemResponse(problem);
        return response;
    }
    
    public static  Task<HttpResponseData> InternalServerError(this HttpRequestData request, string errorDetail)
    {
        return request.CreateErrorDetailResponse(new InternalErrorEntityProblemDetail(request.Url.AbsoluteUri, errorDetail));
    }

    public static  Task<HttpResponseData> UnprocessableEntity(this HttpRequestData request,
        ImmutableDictionary<string, string[]> errors)
    {
        return request.CreateErrorDetailResponse(new UnprocessableEntityProblemDetail(request.Url.AbsoluteUri, errors));
    }
    
    public static Task<HttpResponseData> UnprocessableEntity(this HttpRequestData request) {
        return request.CreateErrorDetailResponse(new EmptyUnprocessableEntityProblemDetail(request.Url.AbsoluteUri));
    }

    public static Task<HttpResponseData> NotFound(this HttpRequestData request, string detail)
    {
        return request.CreateErrorDetailResponse(new NotFoundProblemDetail(request.Url.AbsoluteUri, detail));
    }

    public static Task<HttpResponseData> Ok(this HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        return Task.FromResult(response);
    }
    
    public static async Task<HttpResponseData> Ok<T>(this HttpRequestData request, T result)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }

    public static Task<HttpResponseData> Unauthorized(this HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.Unauthorized);
        return Task.FromResult(response);
    }

    public static async Task<Either<DomainError,IRequest<T>>> WithAuthenticationCheck<T>(this HttpRequestData request,
        IRequest<T> command)
    {
        var principal = await ClientPrincipal.ParseFromRequest(request);
        return principal.Identity.IsAuthenticated ? Right<DomainError,IRequest<T>>(command) : Left<DomainError,IRequest<T>>(UnauthorizedError.Create(request.Url.AbsoluteUri));
    }
}