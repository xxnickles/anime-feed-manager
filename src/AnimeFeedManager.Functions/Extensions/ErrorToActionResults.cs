using System.Web.Http;
using AnimeFeedManager.Core.Error;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            log.LogError($"[{error.CorrelationId}] {error.Message}");
            foreach (var validationError in error.Errors)
                log.LogError($"Field: {validationError.Field}, Message: {validationError.Description}");

            return new UnprocessableEntityObjectResult(new
            {
                error.Message, error.Errors
            });
        }

        public static IActionResult ToActionResult(this ExceptionError error, ILogger log)
        {
            log.LogError(error.ToString());
            return new InternalServerErrorResult();
        }
    }
}