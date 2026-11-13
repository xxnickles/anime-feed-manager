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
                activity.SetTag(Telemetry.ErrorTypeTag, ErrorTypeName(error.GetType()));
            });
    }

    extension<T, TError>(Task<Result<T, TError>> resultTask)
    {
        public async Task<Result<T, TError>> MarkActivityErroredOnError() =>
            (await resultTask).MarkActivityErroredOnError();
    }

    // A leaf nested in its family keeps the family in the name: StorageError.NotFound, not NotFound.
    private static string ErrorTypeName(Type type) =>
        type.DeclaringType is { } family ? $"{family.Name}.{type.Name}" : type.Name;

    private static bool IsSpanError(DomainError error) =>
        error is not (Warning or DomainValidationErrors or FormDataValidationError);
}
