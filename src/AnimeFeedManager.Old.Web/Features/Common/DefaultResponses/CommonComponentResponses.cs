using AnimeFeedManager.Old.Common.Domain.Errors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Old.Web.Features.Common.DefaultResponses;

internal static class CommonComponentResponses
{
   internal static RazorComponentResult OkComponentResult(string message)
   {
      var parameters = new Dictionary<string, object?>
      {
         {nameof(OkResult.OkMessage), message}
      };
      return new RazorComponentResult<OkResult>(parameters);
   }
   
   internal static RazorComponentResult ErrorComponentResult(DomainError error, ILogger logger)
   {
      error.LogError(logger);
      var parameters = new Dictionary<string, object?>
      {
         {nameof(ErrorResult.Error), error}
      };
      return new RazorComponentResult<ErrorResult>(parameters);
   }
}