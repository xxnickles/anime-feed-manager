using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public abstract record LastScrape;

public sealed record NoScrape : LastScrape;

public sealed record LatestScrape(
    DateTimeOffset At,
    SeriesSeason Season,
    int NewSeries,
    int UpdatedSeries,
    int Runs) : LastScrape;

public static class LastScrapeStat
{
    public static Task<Result<LastScrape, StorageError>> Get(
        SystemEventsGetter<ScrapTvLibraryResult> eventGetter,
        DateTimeOffset from,
        CancellationToken cancellationToken) =>
        eventGetter(TargetConsumer.Everybody(), from, cancellationToken).Map(Summarize);

    internal static LastScrape Summarize(ImmutableArray<EventData<ScrapTvLibraryResult>> events)
    {
        if (events.IsEmpty) return new NoScrape();

        var latest = events.MaxBy(e => e.Timestamp)!;
        return new LatestScrape(latest.Timestamp, latest.Data.Season, latest.Data.NewSeries,
            latest.Data.UpdatedSeries, events.Length);
    }
}
