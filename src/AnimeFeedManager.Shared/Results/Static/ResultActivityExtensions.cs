using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

public static class ResultActivityExtensions
{
    extension<T, TError>(Result<T, TError> result)
    {
        /// <summary>
        /// On failure, marks <see cref="Activity.Current"/> errored and tags the error type; client-input errors
        /// (<see cref="Warning"/>, validation) are skipped. Call after any recovery, inside the method that owns the activity.
        /// </summary>
        public Result<T, TError> MarkActivityErroredOnError() =>
            result.TapError(caseError =>
            {
                var error = ErrorCases.AsDomainError(caseError);
                if (Activity.Current is not { } activity || !IsSpanError(error))
                    return;

                activity.SetStatus(ActivityStatusCode.Error,
                    string.IsNullOrEmpty(error.Message) ? null : error.Message);
                activity.SetTag(Telemetry.ErrorTypeTag, error.GetType().Name);
            });
    }

    extension<T, TError>(Task<Result<T, TError>> resultTask)
    {
        public async Task<Result<T, TError>> MarkActivityErroredOnError() =>
            (await resultTask).MarkActivityErroredOnError();
    }

    private static bool IsSpanError(DomainError error) =>
        error is not (Warning or DomainValidationErrors or FormDataValidationError);
}
