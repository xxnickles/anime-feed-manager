using AnimeFeedManager.Features.Scrapping;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Seasons.Queries;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

[Widens<LatestSeasonError>]
public partial union AutomatedBindingError(StorageError, NoLatestSeason, DeliveryError);

/// <summary>The SubsPlease schedule, or why it couldn't be read.</summary>
internal union ScheduleRead(ImmutableArray<ScheduledShow>, ScrapeError);

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
    public static Task<Result<AutomatedBindingCompleted, AutomatedBindingError>> Run(
        ISubsPleaseSchedule schedule,
        ITableClientFactory clientFactory,
        DelayedMessageSender delayedSender,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        ReadSchedule(schedule, token)
            .Then(Result<ScheduleRead, AutomatedBindingError>.Success)
            .StartProcess(clientFactory, token)
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

    private static Task<ScheduleRead> ReadSchedule(ISubsPleaseSchedule schedule, CancellationToken token) =>
        schedule.Fetch(token).MatchToValue(ScheduleRead (shows) => shows, error => error);

    extension(Task<Result<ScheduleRead, AutomatedBindingError>> read)
    {
        private Task<Result<AutomatedBindingProcess, AutomatedBindingError>> StartProcess(
            ITableClientFactory clientFactory,
            CancellationToken token) =>
            read.Bind(schedule => schedule switch
            {
                ImmutableArray<ScheduledShow> shows => Candidates(clientFactory, token)
                    .Map(candidates => new AutomatedBindingProcess(shows, ScheduleSkipped: false) {Candidates = candidates}),
                ScrapeError error => Task.FromResult(Result<AutomatedBindingProcess, LatestSeasonError>
                    .Success(new AutomatedBindingProcess([], ScheduleSkipped: true))
                    .AddLog(ScheduleSkipped(error)))
            });
    }

    extension(Task<Result<AutomatedBindingProcess, AutomatedBindingError>> process)
    {

        private Task<Result<AutomatedBindingProcess, AutomatedBindingError>> MatchSchedule() =>
            process.Map(p => p with {Matches = ScheduleMatching.Match(p.Schedule, p.Candidates)})
                .AddLogOnSuccess(p => LogTies(p.Matches.Ties));

        private Task<Result<AutomatedBindingProcess, AutomatedBindingError>> SendBinds(DelayedMessageSender delayedSender,
            CancellationToken token) =>
            process.Bind(p => p.Matches.Matches.IsEmpty
                ? Task.FromResult(Result<AutomatedBindingProcess, DeliveryError>.Success(p))
                : Send(p.Matches.Matches, delayedSender, token).Map(sent => p with {Sent = sent}));

        private Task<Result<AutomatedBindingCompleted, AutomatedBindingError>> StoreRun(AutomatedBindingRunUpdater runUpdater,
            CancellationToken token) =>
            process.Map(p => new AutomatedBindingCompleted(DateTimeOffset.UtcNow, p.Schedule.Length, p.Sent,
                    p.Matches.Ties, p.ScheduleSkipped))
                .Bind(run => runUpdater(run, token).Map(_ => run));
    }

    extension(Task<Result<AutomatedBindingCompleted, AutomatedBindingError>> run)
    {
        private Task<Result<AutomatedBindingCompleted, AutomatedBindingError>> SendSummary(DomainCollectionSender domainPostman,
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
    private static Task<Result<ImmutableArray<BindingCandidate>, LatestSeasonError>> Candidates(ITableClientFactory clientFactory,
        CancellationToken token) =>
        SeasonQueries.GetLatestSeason(clientFactory.TableStorageLatestSeason, token)
            .Bind(season => clientFactory.TableStorageRawExistentStoredSeries()(season, token))
            .Bind(latest => clientFactory.TableStorageOnGoingStoredTvSeries(token)
                .Map(ongoing => Unfinished([.. latest, .. ongoing])))
            .Bind(series => BoundSeriesIds(series, clientFactory.TableStorageSeasonNyaaBindings, token)
                .Map(bound => series
                    .Where(entry => !bound.Contains(entry.RowKey ?? string.Empty))
                    .Select(ToCandidate)
                    .ToImmutableArray()));

    private static ImmutableArray<AnimeInfoStorage> Unfinished(IEnumerable<AnimeInfoStorage> series) =>
    [
        .. series
            .Where(entry => !string.IsNullOrEmpty(entry.RowKey) && entry.Status != SeriesStatus.CompletedValue)
            .DistinctBy(entry => entry.RowKey)
    ];

    private static async Task<Result<HashSet<string>, StorageError>> BoundSeriesIds(
        ImmutableArray<AnimeInfoStorage> series,
        SeasonNyaaBindings bindingsGetter,
        CancellationToken token)
    {
        var seasons = series
            .Select(entry => IdHelpers.SeriesSeasonFromId(entry.RowKey ?? string.Empty)
                .MapError(StorageError (fields) => new StorageError.InvalidRecord(typeof(AnimeInfoStorage), fields.Errors)))
            .CollectAll(parsed => parsed
                    .DistinctBy(IdHelpers.GenerateAnimePartitionKey)
                    .ToImmutableArray(),
                StorageError (errors) => new StorageError.BatchFailed("Some series' seasons could not be read", errors));

        return await seasons.Bind(async distinct =>
            (await Task.WhenAll(distinct.Select(season => bindingsGetter(season, token))))
            .CollectAll(bindings => bindings.SelectMany(rows => rows).Select(row => row.RowKey ?? string.Empty).ToHashSet(),
                StorageError (errors) => new StorageError.BatchFailed("Some series' bindings could not be read", errors)));
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

    private static async Task<Result<int, DeliveryError>> Send(
        ImmutableArray<ScheduleMatch> matches,
        DelayedMessageSender delayedSender,
        CancellationToken token)
    {
        var sends = await Task.WhenAll(matches.Select((match, index) =>
            delayedSender(BindRequest(match), NyaaPacing.For(index), token)));

        return sends
            .Collect(sent => sent.Count(),
                DeliveryError (errors) => new DeliveryError.BatchFailed("No bind request could be sent", errors))
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

    private static Action<ILogger> ScheduleSkipped(ScrapeError error) =>
        logger => logger.LogWarning(
            "SubsPlease schedule unavailable, automated binding skipped: {domain_error.message}", error.Message);
}
