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
    public static Task<Result<LastScrape>> Get(
        SystemEventsGetter<ScrapTvLibraryResult> eventGetter,
        DateTimeOffset from,
        CancellationToken cancellationToken) =>
        eventGetter(TargetConsumer.Everybody(), from, cancellationToken).Map(Summarize);

    // Titles scrapes share the payload type but don't touch the library.
    internal static LastScrape Summarize(ImmutableArray<EventData<ScrapTvLibraryResult>> events)
    {
        var runs = events.Where(e => e.Data.UpdateType == UpdateType.FullLibrary).ToImmutableArray();
        if (runs.IsEmpty) return new NoScrape();

        var latest = runs.MaxBy(e => e.Timestamp)!;
        return new LatestScrape(latest.Timestamp, latest.Data.Season, latest.Data.NewSeries,
            latest.Data.UpdatedSeries, runs.Length);
    }
}
