namespace AnimeFeedManager.Features.Feeds.Events;

/// <summary>
/// Raised once an airing-clock run completes with at least one flagged release — a
/// fire-and-forget domain event fed to the admin SSE toast. Success-shaped only, matching
/// <see cref="NyaaCollectionRunCompleted"/>'s precedent; a run that flags nothing stays silent.
/// No <c>Source</c> field — unlike the Nyaa hot/cold split, this is always one job.
/// </summary>
public sealed record AiringClockCheckRunCompleted(int ItemsScanned, int FlaggedCount, int UnmatchedCount);
