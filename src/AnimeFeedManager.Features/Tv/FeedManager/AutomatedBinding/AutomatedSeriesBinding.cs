using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

internal sealed record AutomatedBindingProcess(Guid RunId, ImmutableArray<ScheduledShow> Schedule, bool ScheduleSkipped)
{
    public ImmutableArray<BindingCandidate> Candidates { get; init; } = [];
    public ScheduleMatches Matches { get; init; } = ScheduleMatches.None;
    public int Sent { get; init; }
}

public static class AutomatedSeriesBinding
{
    private const string SubsPleaseGroup = "SubsPlease";
    private const string SubsPleaseVariant = "(1080p)";
    private const int GroupSize = 5;
    private static readonly TimeSpan GroupInterval = TimeSpan.FromSeconds(10);

    /// <summary>Binds unbound, unfinished series from the SubsPlease schedule and stores the run summary; a failed or empty schedule skips binding with a warning.</summary>
    public static Task<Result<AutomatedBindingCompleted>> Run(
        ISubsPleaseSchedule schedule,
        ITableClientFactory clientFactory,
        DelayedMessageSender delayedSender,
        DomainCollectionSender domainPostman,
        CancellationToken token)
    {
        var runId = Guid.NewGuid();
        return StartProcess(runId, schedule, token)
            .LoadCandidates(clientFactory, token)
            .MatchSchedule()
            .SendBinds(delayedSender, token)
            .SendSummary(domainPostman, token)
            .SendOnError(domainPostman,
                [
                    new SystemEvent(TargetConsumer.Admin(), EventTarget.Both, EventType.Error,
                        new AutomatedBindingFailed(runId).AsEventPayload())
                ],
                token)
            .WithOperationName(nameof(Run));
    }

    private static Task<Result<AutomatedBindingProcess>> StartProcess(Guid runId, ISubsPleaseSchedule schedule,
        CancellationToken token) =>
        schedule.Fetch(token)
            .Map(shows => new AutomatedBindingProcess(runId, shows, ScheduleSkipped: false))
            .BindOnError(error => Result<AutomatedBindingProcess>
                .Success(new AutomatedBindingProcess(runId, [], ScheduleSkipped: true))
                .AddLog(ScheduleSkipped(error)));

    extension(Task<Result<AutomatedBindingProcess>> process)
    {
        private Task<Result<AutomatedBindingProcess>> LoadCandidates(ITableClientFactory clientFactory,
            CancellationToken token) =>
            process.Bind(p => p.Schedule.IsEmpty
                ? Task.FromResult(Result<AutomatedBindingProcess>.Success(p))
                : Candidates(clientFactory, token).Map(candidates => p with {Candidates = candidates}));

        private Task<Result<AutomatedBindingProcess>> MatchSchedule() =>
            process.Map(p => p with {Matches = ScheduleMatching.Match(p.Schedule, p.Candidates)})
                .AddLogOnSuccess(p => LogTies(p.Matches.Ties));

        private Task<Result<AutomatedBindingProcess>> SendBinds(DelayedMessageSender delayedSender,
            CancellationToken token) =>
            process.Bind(p => p.Matches.Matches.IsEmpty
                ? Task.FromResult(Result<AutomatedBindingProcess>.Success(p))
                : Send(p.RunId, p.Matches.Matches, delayedSender, token).Map(sent => p with {Sent = sent}));

        private Task<Result<AutomatedBindingCompleted>> SendSummary(DomainCollectionSender domainPostman,
            CancellationToken token) =>
            process.Map(p => new AutomatedBindingCompleted(p.RunId, p.Schedule.Length, p.Sent, p.Matches.Ties,
                    p.ScheduleSkipped))
                .Bind(summary => domainPostman([SummaryEvent(summary)], token).Map(_ => summary));
    }

    // A run with nothing to act on is stored for the stat card without a toast.
    internal static SystemEvent SummaryEvent(AutomatedBindingCompleted summary) =>
        new(TargetConsumer.Admin(),
            summary.Queued > 0 || !summary.Ties.IsEmpty || summary.ScheduleSkipped
                ? EventTarget.Both
                : EventTarget.LocalStorage,
            EventType.Completed,
            summary.AsEventPayload());

    // Latest season plus carry-overs; a season whose bindings can't be read fails the run, since its
    // series would read as unbound and a manual binding could be replaced.
    private static Task<Result<ImmutableArray<BindingCandidate>>> Candidates(ITableClientFactory clientFactory,
        CancellationToken token) =>
        LatestSeason(clientFactory.TableStorageLatestSeason, token)
            .Bind(season => clientFactory.TableStorageRawExistentStoredSeries()(season, token))
            .Bind(latest => clientFactory.TableStorageOnGoingStoredTvSeries(token)
                .Map(ongoing => Unfinished([.. latest, .. ongoing])))
            .Bind(series => BoundSeriesIds(series, clientFactory.TableStorageSeasonNyaaBindings, token)
                .Map(bound => series
                    .Where(entry => !bound.Contains(entry.RowKey ?? string.Empty))
                    .Select(ToCandidate)
                    .ToImmutableArray()));

    private static Task<Result<SeriesSeason>> LatestSeason(LatestSeasonGetter seasonGetter, CancellationToken token) =>
        seasonGetter(token).Bind(season => season switch
        {
            CurrentLatestSeason latest => SeasonOf(latest.Season),
            FallbackLatestSeason newest => SeasonOf(newest.Season),
            _ => Error.Create("There is no latest season data in storage.")
        });

    private static Result<SeriesSeason> SeasonOf(SeasonStorage season) =>
        (season.Season ?? string.Empty, season.Year, season.Latest).ParseAsSeriesSeason();

    private static ImmutableArray<AnimeInfoStorage> Unfinished(IEnumerable<AnimeInfoStorage> series) =>
    [
        .. series
            .Where(entry => !string.IsNullOrEmpty(entry.RowKey) && entry.Status != SeriesStatus.CompletedValue)
            .DistinctBy(entry => entry.RowKey)
    ];

    private static async Task<Result<HashSet<string>>> BoundSeriesIds(
        ImmutableArray<AnimeInfoStorage> series,
        SeasonNyaaBindings bindingsGetter,
        CancellationToken token)
    {
        var seasons = series
            .Select(entry => IdHelpers.SeriesSeasonFromId(entry.RowKey ?? string.Empty))
            .Flatten(parsed => parsed
                .DistinctBy(season => IdHelpers.GenerateAnimePartitionKey(season))
                .ToImmutableArray())
            .Bind(Complete);

        return await seasons.Bind(async distinct =>
            (await Task.WhenAll(distinct.Select(season => bindingsGetter(season, token))))
            .Flatten(bindings => bindings.SelectMany(rows => rows).Select(row => row.RowKey ?? string.Empty).ToHashSet())
            .Bind(Complete));
    }

    private static Result<T> Complete<T>(BulkResult<T> bulk) =>
        bulk is PartialSuccessBulkResult<T> partial
            ? new AggregatedError("Some series' bindings could not be read", partial.Errors)
            : bulk.Value;

    private static BindingCandidate ToCandidate(AnimeInfoStorage series) =>
        new(series.RowKey ?? string.Empty,
            series.Title ?? string.Empty,
            series.FeedTitle ?? string.Empty,
            [
                .. new[] {series.Title}
                    .Concat(StoredAlternativeTitles.Parse(series.AlternativeTitles).ForMatching)
                    .Where(title => !string.IsNullOrWhiteSpace(title))
                    .Select(title => title!)
            ]);

    private static async Task<Result<int>> Send(
        Guid runId,
        ImmutableArray<ScheduleMatch> matches,
        DelayedMessageSender delayedSender,
        CancellationToken token)
    {
        var sends = await Task.WhenAll(matches.Select((match, index) =>
            delayedSender(BindRequest(runId, match), Paced(index), token)));

        return sends
            .Flatten(sent => sent.Count())
            .AddLogOnSuccess(bulk => bulk.LogErrors)
            .Map(bulk => bulk.Value);
    }

    internal static Delay Paced(int index) => new(GroupInterval * (index / GroupSize));

    private static BindNyaaSeries BindRequest(Guid runId, ScheduleMatch match) =>
        new(match.SeriesId, SubsPleaseGroup, match.ShowTitle, SubsPleaseVariant, Trusted: true, runId,
            BindingOrigin.Schedule);

    private static Action<ILogger> LogTies(ImmutableArray<ScheduleTie> ties) => logger =>
    {
        foreach (var tie in ties)
            logger.LogWarning("Automated binding left {feed.title} unbound: it ties on {series.ids}",
                tie.ShowTitle, string.Join(",", tie.Series.Select(series => series.SeriesId)));
    };

    private static Action<ILogger> ScheduleSkipped(DomainError error) =>
        logger => logger.LogWarning(
            "SubsPlease schedule unavailable, automated binding skipped: {domain_error.message}", error.Message);
}
