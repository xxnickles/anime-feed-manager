using System.Collections.Immutable;
using System.Net;
using System.Security.Claims;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Functions.ResponseExtensions;

public static class HttpRequestDataExtensions
{

    public static ValueTask WriteAsProblemResponse<T>(this HttpResponseData response, T detail, CancellationToken cancellationToken = default) where T : ProblemDetails
    {
        return response.WriteAsJsonAsync((object)detail, "application/problem+json", detail.Status, cancellationToken);
    }

    private static async Task<HttpResponseData> CreateErrorDetailResponse(this HttpRequestData request,
        ProblemDetails problem)
    {
        var response = request.CreateResponse();
        await response.WriteAsProblemResponse(problem);
        return response;
    }

    public static Task<HttpResponseData> InternalServerError(this HttpRequestData request, string errorDetail)
    {
        return request.CreateErrorDetailResponse(new InternalErrorEntityProblemDetail(request.Url.AbsoluteUri, errorDetail));
    }

    public static Task<HttpResponseData> UnprocessableEntity(this HttpRequestData request,
        ImmutableDictionary<string, string[]> errors)
    {
        return request.CreateErrorDetailResponse(new UnprocessableEntityProblemDetail(request.Url.AbsoluteUri, errors));
    }

    public static Task<HttpResponseData> UnprocessableEntity(this HttpRequestData request)
    {
        return request.CreateErrorDetailResponse(new EmptyUnprocessableEntityProblemDetail(request.Url.AbsoluteUri));
    }

    public static Task<HttpResponseData> NotFound(this HttpRequestData request, string detail)
    {
        return request.CreateErrorDetailResponse(new NotFoundProblemDetail(request.Url.AbsoluteUri, detail));
    }

    public static Task<HttpResponseData> Accepted(this HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.Accepted);
        return Task.FromResult(response);
    }
    
    public static Task<HttpResponseData> NoContent(this HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.NoContent);
        return Task.FromResult(response);
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

    public static Task<HttpResponseData> Forbidden(this HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.Forbidden);
        return Task.FromResult(response);
    }

    public static Task<Either<DomainError, Unit>> WithRoleCheck(this HttpRequestData request, string role)
    {
        return request.CheckAuthorization().BindAsync((p) =>
        {
            var (principal, r) = p;
            return principal.IsInRole(role) ? Right<DomainError, Unit>(unit) : Left<DomainError, Unit>(ForbiddenError.Create(request.Url.AbsoluteUri));
        });
    }

    public static Task<Either<DomainError, Unit>> AllowAdminOnly(this HttpRequestData request)
    {
        return request.WithRoleCheck(UserRoles.Admin);
    }

    public static async Task<Either<DomainError, (ClaimsPrincipal principal, HttpRequestData request)>> CheckAuthorization(this HttpRequestData request)
    {
        var principal = await ClientPrincipal.ParseFromRequest(request);
        return principal.Identity?.IsAuthenticated is true ? Right<DomainError, (ClaimsPrincipal, HttpRequestData)>((principal, request)) : Left<DomainError, (ClaimsPrincipal, HttpRequestData)>(UnauthorizedError.Create(request.Url.AbsoluteUri));
    }

}