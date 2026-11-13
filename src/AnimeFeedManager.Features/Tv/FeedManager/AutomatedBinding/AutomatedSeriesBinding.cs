using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

internal sealed record AutomatedBindingProcess(ImmutableArray<ScheduledShow> Schedule, bool ScheduleSkipped)
{
    public ImmutableArray<BindingCandidate> Candidates { get; init; } = [];
    public ScheduleMatches Matches { get; init; } = ScheduleMatches.None;
    public int Sent { get; init; }
}

public static class AutomatedSeriesBinding
{
    private const string SubsPleaseGroup = "SubsPlease";
    private const string SubsPleaseVariant = "(1080p)";

    /// <summary>App event subject the Automated binding card listens for.</summary>
    public const string RunSubject = "latest";

    /// <summary>Binds unbound, unfinished series from the SubsPlease schedule, stores the run and toasts it; a failed or empty schedule skips binding with a warning.</summary>
    public static Task<Result<AutomatedBindingCompleted, DomainError>> Run(
        ISubsPleaseSchedule schedule,
        ITableClientFactory clientFactory,
        DelayedMessageSender delayedSender,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        StartProcess(schedule, token)
            .LoadCandidates(clientFactory, token)
            .MatchSchedule()
            .SendBinds(delayedSender, token)
            .StoreRun(clientFactory.TableStorageAutomatedBindingRunUpdater, token)
            .SendSummary(domainPostman, token)
            .SendOnError(domainPostman,
                [
                    new SystemEvent(TargetConsumer.Admin(), EventTarget.Both, EventType.Error,
                        new AutomatedBindingFailed().AsEventPayload())
                ],
                token)
            .WithOperationName(nameof(Run));

    private static Task<Result<AutomatedBindingProcess, DomainError>> StartProcess(ISubsPleaseSchedule schedule,
        CancellationToken token) =>
        schedule.Fetch(token)
            .Map(shows => new AutomatedBindingProcess(shows, ScheduleSkipped: false))
            .BindOnError(error => Result<AutomatedBindingProcess, DomainError>
                .Success(new AutomatedBindingProcess([], ScheduleSkipped: true))
                .AddLog(ScheduleSkipped(error)));

    extension(Task<Result<AutomatedBindingProcess, DomainError>> process)
    {
        private Task<Result<AutomatedBindingProcess, DomainError>> LoadCandidates(ITableClientFactory clientFactory,
            CancellationToken token) =>
            process.Bind(p => p.Schedule.IsEmpty
                ? Task.FromResult(Result<AutomatedBindingProcess, DomainError>.Success(p))
                : Candidates(clientFactory, token).Map(candidates => p with {Candidates = candidates}));

        private Task<Result<AutomatedBindingProcess, DomainError>> MatchSchedule() =>
            process.Map(p => p with {Matches = ScheduleMatching.Match(p.Schedule, p.Candidates)})
                .AddLogOnSuccess(p => LogTies(p.Matches.Ties));

        private Task<Result<AutomatedBindingProcess, DomainError>> SendBinds(DelayedMessageSender delayedSender,
            CancellationToken token) =>
            process.Bind(p => p.Matches.Matches.IsEmpty
                ? Task.FromResult(Result<AutomatedBindingProcess, BulkError<DeliveryError>.AllFailed>.Success(p))
                : Send(p.Matches.Matches, delayedSender, token).Map(sent => p with {Sent = sent}));

        private Task<Result<AutomatedBindingCompleted, DomainError>> StoreRun(AutomatedBindingRunUpdater runUpdater,
            CancellationToken token) =>
            process.Map(p => new AutomatedBindingCompleted(DateTimeOffset.UtcNow, p.Schedule.Length, p.Sent,
                    p.Matches.Ties, p.ScheduleSkipped))
                .Bind(run => runUpdater(run, token).Map(_ => run));
    }

    extension(Task<Result<AutomatedBindingCompleted, DomainError>> run)
    {
        private Task<Result<AutomatedBindingCompleted, DomainError>> SendSummary(DomainCollectionSender domainPostman,
            CancellationToken token) =>
            run.Bind(summary => domainPostman(SummaryEvents(summary), token).Map(_ => summary));
    }

    // The run itself is the stored record, so the toast isn't stored again.
    internal static DomainMessage[] SummaryEvents(AutomatedBindingCompleted summary) =>
    [
        new SystemEvent(TargetConsumer.Admin(), EventTarget.Browser, EventType.Completed, summary.AsEventPayload()),
        new AppEvent(TargetConsumer.Admin(), AppEventNames.AutomatedBindingFinished, RunSubject)
    ];

    // Latest season plus carry-overs; a season whose bindings can't be read fails the run, since its
    // series would read as unbound and a manual binding could be replaced.
    private static Task<Result<ImmutableArray<BindingCandidate>, DomainError>> Candidates(ITableClientFactory clientFactory,
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

    private static Task<Result<SeriesSeason, DomainError>> LatestSeason(LatestSeasonGetter seasonGetter, CancellationToken token) =>
        seasonGetter(token).MapError(error => (DomainError)error)
            .Bind(stored => stored.AsFeaturedSeason())
            .Bind(SeasonOf);

    private static Result<SeriesSeason, StorageError> SeasonOf(SeasonStorage season) =>
        (season.Season ?? string.Empty, season.Year, season.Latest).ParseAsSeriesSeason()
            .MapError(StorageError (fields) => new StorageError.InvalidRecord(typeof(SeasonStorage), fields.Errors));

    private static ImmutableArray<AnimeInfoStorage> Unfinished(IEnumerable<AnimeInfoStorage> series) =>
    [
        .. series
            .Where(entry => !string.IsNullOrEmpty(entry.RowKey) && entry.Status != SeriesStatus.CompletedValue)
            .DistinctBy(entry => entry.RowKey)
    ];

    private static async Task<Result<HashSet<string>, BulkError<StorageError>>> BoundSeriesIds(
        ImmutableArray<AnimeInfoStorage> series,
        SeasonNyaaBindings bindingsGetter,
        CancellationToken token)
    {
        var seasons = series
            .Select(entry => IdHelpers.SeriesSeasonFromId(entry.RowKey ?? string.Empty)
                .MapError(StorageError (fields) => new StorageError.InvalidRecord(typeof(AnimeInfoStorage), fields.Errors)))
            .Collect(parsed => parsed
                .DistinctBy(IdHelpers.GenerateAnimePartitionKey)
                .ToImmutableArray())
            .RequireAll("Some series' seasons could not be read");

        return await seasons.Bind(async distinct =>
            (await Task.WhenAll(distinct.Select(season => bindingsGetter(season, token))))
            .Collect(bindings => bindings.SelectMany(rows => rows).Select(row => row.RowKey ?? string.Empty).ToHashSet())
            .RequireAll("Some series' bindings could not be read"));
    }

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

    private static async Task<Result<int, BulkError<DeliveryError>.AllFailed>> Send(
        ImmutableArray<ScheduleMatch> matches,
        DelayedMessageSender delayedSender,
        CancellationToken token)
    {
        var sends = await Task.WhenAll(matches.Select((match, index) =>
            delayedSender(BindRequest(match), NyaaPacing.For(index), token)));

        return sends
            .Collect(sent => sent.Count())
            .AddLogOnSuccess(bulk => bulk.LogErrors)
            .Map(bulk => bulk.Value);
    }

    private static BindNyaaSeries BindRequest(ScheduleMatch match) =>
        new(match.SeriesId, SubsPleaseGroup, match.ShowTitle, SubsPleaseVariant, Trusted: true, BindingOrigin.Schedule);

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
