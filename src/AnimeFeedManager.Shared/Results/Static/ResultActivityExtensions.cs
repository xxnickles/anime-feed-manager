using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>Bridges <see cref="Result{T}"/> failure into the active <see cref="Activity"/>'s OTel error status.</summary>
public static class ResultActivityExtensions
{
    extension<T>(Result<T> result)
    {
        /// <summary>
        /// On failure, sets <see cref="Activity.Current"/>'s status to <see cref="ActivityStatusCode.Error"/> and tags
        /// <see cref="Telemetry.ErrorTypeTag"/> with the error's class name. Skips <see cref="Warning"/> and validation
        /// errors (client input, like HTTP 4xx); call after any recovery so recovered failures stay unmarked.
        /// </summary>
        /// <remarks>
        /// <see cref="Activity.Current"/> is <c>AsyncLocal</c>, so it resolves to the caller's
        /// <c>using var activity</c> as long as the chain stays inside that method.
        /// </remarks>
        public Result<T> MarkActivityErroredOnError() =>
            result.TapError(error =>
            {
                if (Activity.Current is not { } activity || !IsSpanError(error))
                    return;

                activity.SetStatus(ActivityStatusCode.Error,
                    string.IsNullOrEmpty(error.Message) ? null : error.Message);
                activity.SetTag(Telemetry.ErrorTypeTag, error.GetType().Name);
            });
    }

    extension<T>(Task<Result<T>> resultTask)
    {
        /// <inheritdoc cref="MarkActivityErroredOnError{T}(Result{T})"/>
        public async Task<Result<T>> MarkActivityErroredOnError() =>
            (await resultTask).MarkActivityErroredOnError();
    }

    private static bool IsSpanError(DomainError error) =>
        error is not (Warning or DomainValidationErrors or FormDataValidationError);
}
