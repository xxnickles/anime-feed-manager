namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Bridges <see cref="Result{T}"/> failure into the active <see cref="Activity"/>'s OTel status,
/// so trace-view dashboards (Aspire, Grafana, App Insights) render failed orchestrators as
/// errored rather than neutral. Layered on top of the generic <c>TapError</c> combinators in
/// <see cref="ResultExtensionMembers"/> / <see cref="ResultTaskExtensionMembers"/>.
/// </summary>
public static class ResultActivityExtensions
{
    extension<T>(Result<T> result)
    {
        /// <summary>
        /// On failure, sets <see cref="Activity.Current"/>'s status to
        /// <see cref="ActivityStatusCode.Error"/> with the <see cref="DomainError"/>'s message as
        /// description. No-op when no activity is current or the result is successful. Returns
        /// the result unchanged.
        /// </summary>
        /// <remarks>
        /// Relies on <see cref="Activity.Current"/> resolving to the orchestrator's
        /// <c>using var activity</c> scope. <see cref="Activity.Current"/> is <c>AsyncLocal</c>
        /// and flows across awaits, so the right span is in scope as long as the chain
        /// composition stays inside the orchestrator method.
        /// </remarks>
        public Result<T> MarkActivityErroredOnError() =>
            result.TapError(error =>
            {
                if (Activity.Current is { } activity)
                    activity.SetStatus(ActivityStatusCode.Error, error.Message);
            });
    }

    extension<T>(Task<Result<T>> resultTask)
    {
        /// <inheritdoc cref="MarkActivityErroredOnError{T}(Result{T})"/>
        public async Task<Result<T>> MarkActivityErroredOnError() =>
            (await resultTask).MarkActivityErroredOnError();
    }
}
