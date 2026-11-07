namespace AnimeFeedManager.Features.Tv.Library.Storage.Stores;

public abstract record ExpectedEpisodes;

public sealed record KnownEpisodes(int Count) : ExpectedEpisodes;

/// <summary>No count yet; the series stays open until the provider or an admin supplies one.</summary>
public sealed record UnknownEpisodes : ExpectedEpisodes;

public abstract record ReleaseSlot;

/// <summary><paramref name="At"/> is the first sub release (UTC); its weekday and time anchor the weekly slot.</summary>
public sealed record FirstRelease(DateTime At) : ReleaseSlot;

public sealed record UnknownRelease : ReleaseSlot;

public sealed record StreamLink(string Platform, string Name, Uri Url);

public sealed record BroadcastHint(ReleaseSlot When, ImmutableArray<StreamLink> Where);

internal static class SeriesBroadcast
{
    // Streams lead TV by up to ~6 days; pre-season sub times are placeholders months before the premiere
    private static readonly TimeSpan StreamAheadAllowance = TimeSpan.FromDays(14);

    internal static ExpectedEpisodes ToExpectedEpisodes(int? episodes) =>
        episodes is > 0 ? new KnownEpisodes(episodes.Value) : new UnknownEpisodes();

    internal static BroadcastHint ToBroadcastHint(DateTime? subTime, DateTime? premier, string? streams) =>
        new(ToReleaseSlot(subTime, premier), ToStreamLinks(streams));

    private static ReleaseSlot ToReleaseSlot(DateTime? subTime, DateTime? premier) =>
        subTime is { } at && premier is { } first && at >= first - StreamAheadAllowance
            ? new FirstRelease(at)
            : new UnknownRelease();

    private static ImmutableArray<StreamLink> ToStreamLinks(string? streams) =>
        [
            ..StoredStreams.Parse(streams)
                .Select(ToStreamLink)
                .OfType<StreamLink>()
        ];

    private static StreamLink? ToStreamLink(StoredStream stream) =>
        !string.IsNullOrWhiteSpace(stream.Platform)
        && !string.IsNullOrWhiteSpace(stream.Url)
        && Uri.TryCreate(stream.Url.Contains("://") ? stream.Url : $"https://{stream.Url}", UriKind.Absolute, out var url)
            ? new StreamLink(stream.Platform, string.IsNullOrWhiteSpace(stream.Name) ? stream.Platform : stream.Name, url)
            : null;
}
