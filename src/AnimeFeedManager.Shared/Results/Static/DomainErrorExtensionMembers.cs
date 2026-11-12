namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Provides extension members for <see cref="DomainError"/>,
/// enabling error-to-result conversions and direct error logging.
/// </summary>
public static class DomainErrorExtensionMembers
{
    // ──────────────────────────────────────────────────────────────────
    // Logging
    // ──────────────────────────────────────────────────────────────────

    extension(DomainError error)
    {
        /// <summary>
        /// Writes the error directly to the provided logger using the error's defined log action.
        /// Use this for immediate error logging outside of a result chain.
        /// For logging within result chains, prefer <see cref="ResultExtensionMembers.FlushLogs{T}"/>.
        /// </summary>
        /// <param name="logger">The logger to write the error to.</param>
        public void WriteError(ILogger logger) => error.LogAction().Invoke(logger);
    }
}
