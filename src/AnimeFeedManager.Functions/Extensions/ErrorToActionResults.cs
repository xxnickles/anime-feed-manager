using AnimeFeedManager.Core.Error;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Web.Http;

namespace AnimeFeedManager.Functions.Extensions
{
    public static class ErrorToActionResults
    {

        public static IActionResult ToActionResult(this DomainError error, ILogger log)
        {
            return error switch
            {
                ExceptionError eError => eError.ToActionResult(log),
                ValidationErrors vError => vError.ToActionResult(log),
                _ => new InternalServerErrorResult()
            };
        }

        public static IActionResult ToActionResult(this ValidationErrors error, ILogger log)
        {
            log.LogError($"[{error.CorrelationId}] {error.Error}");
            foreach (var validationError in error.Errors)
            {
                log.LogError($"Source: {validationError.Source}, Error: {validationError.Error}");
            }

            return new BadRequestObjectResult(new{ ValidationErrors = error.Errors });
        }

        public static IActionResult ToActionResult(this ExceptionError error, ILogger log)
        {
            log.LogError(error.ToString());
            return new InternalServerErrorResult();
        }
    }
}
