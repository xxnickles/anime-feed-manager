using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Extensions;

public static class EitherToActionResult
{
    public static Task<IActionResult> ToActionResult<R>(this Task<Either<DomainError, R>> either, ILogger log) =>
        either.Map( x => ToActionResult(x, log));

    public static IActionResult ToActionResult<R>(this Either<DomainError, R> either, ILogger log) =>
        either.Match(
            Left: error => error.ToActionResult(log),
            Right: r => r is Unit ? (IActionResult) new OkResult() : new OkObjectResult(r));


}