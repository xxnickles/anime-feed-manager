namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// Cross-cutting failure signal — any job publishes this on a hard failure. Drives one generic
/// admin error toast; feature-specific persistence subscribers (see <c>AddPersistedEvent</c>)
/// filter by <see cref="Source"/> to persist the ones they own.
/// </summary>
public sealed record OperationFailed(
    string Source,
    string Message,
    DateTimeOffset OccurredAt,
    Outcome Outcome = Outcome.Error);
