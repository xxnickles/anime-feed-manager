using AnimeFeedManager.Features.Auth.Storage;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Feeds.Storage;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Notifications;

/// <summary>
/// Discovers <see cref="ReleaseDetectedStatus.Pending"/> releases, matches them against series
/// subscribers via <see cref="NotificationMatching"/>, and sends one digest email per user.
/// A release is marked <see cref="ReleaseDetectedStatus.Processed"/> only once every digest it
/// appeared in this pass sent successfully — <see cref="SeriesSubscriber.LastNotifiedAt"/> is what
/// actually gates re-matching, so a release left <c>Pending</c> after a failed send is retried next
/// pass, narrowed automatically to just the subscribers who didn't get it. A release matching no
/// subscriber this pass (nothing to send) is left untouched and ages out via the container's TTL.
/// <see cref="render"/> is typed as a delegate specifically so this class stays in
/// <c>Features</c> — its concrete implementation needs a live <c>HtmlRenderer</c>, a Blazor type
/// this project doesn't reference; the Web composition root supplies it.
/// </summary>
public sealed class NotificationDispatchCronJob(
    NotificationEmailRenderer render,
    ICosmosContainerFactory cosmosFactory,
    IOptions<GmailOptions> gmailOptions,
    IOptions<NyaaOptions> nyaaOptions,
    TimeProvider time,
    ILogger<NotificationDispatchCronJob> logger)
{
    private const string DigestSubject = "Your anime update digest";

    private readonly PendingReleaseDetectedLoader _loadPending = cosmosFactory.CosmosPendingReleaseDetectedLoaderHandler();
    private readonly SeriesSubscribersLoader _loadSubscribers = cosmosFactory.CosmosSeriesSubscribersLoaderHandler();
    private readonly UsersByIdsGetter _loadUsers = cosmosFactory.UsersByIdsGetterHandler();
    private readonly ReleaseDetectedUpserter _upsertRelease = cosmosFactory.CosmosReleaseDetectedUpserterHandler();
    private readonly SeriesSubscriberUpserter _upsertSubscriber = cosmosFactory.CosmosSeriesSubscriberUpserterHandler();
    private readonly EmailSender _send = gmailOptions.GmailEmailSenderHandler();

    public async Task Run(CancellationToken cancellationToken)
    {
       await _loadPending(cancellationToken)
           .Bind(releases => Dispatch(releases, cancellationToken))
           .AddLogOnFailure(error => error.LogAction())
           .Complete(logger);
    }

    private Task<Result<Unit>> Dispatch(ImmutableArray<ReleaseDetected> releases, CancellationToken cancellationToken) =>
        releases.Length == 0
            ? Task.FromResult<Result<Unit>>(new Unit())
            : LoadAllSubscribers(releases.Select(r => r.SeriesId).Distinct(), cancellationToken)
                .Bind(subscribers => ResolveAndSend(releases, subscribers, cancellationToken));

    // Per-series query, no cross-partition fan-out; a series whose subscriber load fails is
    // skipped this pass (self-healing next run) rather than failing the whole dispatch.
    private async Task<Result<ImmutableArray<SeriesSubscriber>>> LoadAllSubscribers(
        IEnumerable<int> seriesIds, CancellationToken cancellationToken)
    {
        var results = new List<Result<ImmutableArray<SeriesSubscriber>>>();
        foreach (var seriesId in seriesIds)
            results.Add(await _loadSubscribers(seriesId, cancellationToken));

        return results
            .Flatten(lists => lists.SelectMany(list => list).ToImmutableArray())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<ImmutableArray<SeriesSubscriber>>())
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to load subscribers for this dispatch pass"))
            .AddLogOnFailure(error => error.LogAction())
            .Map(bulk => bulk.Value)
            .BindOnErrorWhen(binder: _ => ImmutableArray<SeriesSubscriber>.Empty, predicate: error => error is AggregatedError);
    }

    private Task<Result<Unit>> ResolveAndSend(
        ImmutableArray<ReleaseDetected> releases, ImmutableArray<SeriesSubscriber> subscribers, CancellationToken cancellationToken)
    {
        var digests = NotificationMatching.AggregateByUser(MatchAllSeries(releases, subscribers));
        if (digests.Length == 0) return Task.FromResult<Result<Unit>>(new Unit());

        var seriesTitles = BuildSeriesTitles(releases);
        var digestDate = time.GetUtcNow();

        return _loadUsers([.. digests.Select(d => d.UserId).Distinct()], cancellationToken)
            .Bind(users => SendAllDigests(digests, users, seriesTitles, digestDate, cancellationToken))
            .Bind(delivered => MarkProcessed(releases, delivered, cancellationToken));
    }

    private static ImmutableArray<UnseenRelease> MatchAllSeries(
        ImmutableArray<ReleaseDetected> releases, ImmutableArray<SeriesSubscriber> subscribers)
    {
        var subscribersBySeries = subscribers.ToLookup(s => s.SeriesId);
        return [.. releases
            .GroupBy(r => r.SeriesId)
            .SelectMany(group => NotificationMatching.MatchSeries([..group], [..subscribersBySeries[group.Key]]))];
    }

    // Releases already carry their series' display title from detection time — no extra lookup.
    private static IReadOnlyDictionary<int, string> BuildSeriesTitles(ImmutableArray<ReleaseDetected> releases) =>
        releases.GroupBy(r => r.SeriesId).ToDictionary(g => g.Key, g => g.First().SeriesTitle);

    private async Task<Result<ImmutableArray<string>>> SendAllDigests(
        ImmutableArray<UserDigest> digests, ImmutableArray<ValidStoredUser> users,
        IReadOnlyDictionary<int, string> seriesTitles, DateTimeOffset digestDate, CancellationToken cancellationToken)
    {
        var results = new List<Result<ImmutableArray<string>>>();
        foreach (var digest in digests)
            results.Add(await SendDigest(digest, users, seriesTitles, digestDate, cancellationToken));

        return results
            .Flatten(lists => lists.SelectMany(ids => ids).ToImmutableArray())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<ImmutableArray<string>>())
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to send this dispatch pass's digests"))
            .AddLogOnFailure(error => error.LogAction())
            .Map(bulk => bulk.Value)
            .BindOnErrorWhen(binder: _ => ImmutableArray<string>.Empty, predicate: error => error is AggregatedError);
    }

    // No stored user for a subscriber is unexpected but not fatal — skip this pass, retry next.
    private Task<Result<ImmutableArray<string>>> SendDigest(
        UserDigest digest, ImmutableArray<ValidStoredUser> users, IReadOnlyDictionary<int, string> seriesTitles,
        DateTimeOffset digestDate, CancellationToken cancellationToken)
    {
        var user = users.FirstOrDefault(u => (string) u.UserId == digest.UserId);
        if (user is null)
            return Task.FromResult<Result<ImmutableArray<string>>>(
                Warning.Create($"No stored user found for subscriber {digest.UserId}; digest skipped this pass"));

        var releaseIds = digest.Series.SelectMany(match => match.Releases).Select(release => release.Id).ToImmutableArray();
        var model = NotificationEmailMapper.Map(digest, user.DisplayName, digestDate, seriesTitles, nyaaOptions.Value);

        return render(model, cancellationToken)
            .Bind(html => _send(user.Email, DigestSubject, html, cancellationToken))
            .Bind(_ => AdvanceMarkers(digest, cancellationToken))
            .Map(_ => releaseIds);
    }

    // Cursor advances only once the digest is confirmed sent; a marker-write failure is logged
    // and swallowed rather than failing the send — the email already reached the subscriber.
    private async Task<Result<Unit>> AdvanceMarkers(UserDigest digest, CancellationToken cancellationToken)
    {
        var results = new List<Result<Unit>>();
        foreach (var match in digest.Series)
            results.Add(await _upsertSubscriber(match.Subscriber with { LastNotifiedAt = match.NewMarker() }, cancellationToken));

        return results
            .Flatten(_ => new Unit())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<Unit>())
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to advance subscriber cursor for user {UserId}", digest.UserId))
            .AddLogOnFailure(error => error.LogAction())
            .Map(bulk => bulk.Value)
            .BindOnErrorWhen(binder: _ => new Unit(), predicate: error => error is AggregatedError);
    }

    private async Task<Result<Unit>> MarkProcessed(
        ImmutableArray<ReleaseDetected> releases, ImmutableArray<string> deliveredIds, CancellationToken cancellationToken)
    {
        if (deliveredIds.Length == 0) return new Unit();

        var byId = releases.ToDictionary(r => r.Id);
        var results = new List<Result<Unit>>();
        foreach (var id in deliveredIds)
            results.Add(await _upsertRelease(byId[id] with { Status = ReleaseDetectedStatus.Processed }, cancellationToken));

        return results
            .Flatten(_ => new Unit())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<Unit>())
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to mark delivered releases processed"))
            .AddLogOnFailure(error => error.LogAction())
            .Map(bulk => bulk.Value)
            .BindOnErrorWhen(binder: _ => new Unit(), predicate: error => error is AggregatedError);
    }
}
