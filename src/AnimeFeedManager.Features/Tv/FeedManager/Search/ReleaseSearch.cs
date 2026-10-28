using System.Xml.Linq;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Queries;

namespace AnimeFeedManager.Features.Tv.FeedManager.Search;

/// <summary>One bindable release: every item sharing group, release title and variant.</summary>
/// <param name="Trusted">Every item is trusted, so a binding to it can accept only trusted items.</param>
/// <param name="Episodes">Distinct episode labels, regular episodes first, in episode order.</param>
public sealed record ReleaseCandidate(
    string Group,
    string ReleaseTitle,
    string Variant,
    bool Trusted,
    ImmutableArray<string> Episodes,
    DateTimeOffset Newest,
    bool Current);

/// <param name="Full">The search filled nyaa's 75-item window, so older releases may be missing.</param>
public sealed record ReleaseSearchResults(ImmutableArray<ReleaseCandidate> Candidates, bool Full);

public static class ReleaseSearch
{
    /// <summary>Searches nyaa and groups single-episode releases into candidates, newest first; batches and unparsable titles are left out.</summary>
    public static Task<Result<ReleaseSearchResults>> Search(
        string terms,
        bool trustedOnly,
        CurrentBinding binding,
        INyaaClient nyaaClient,
        CancellationToken token) =>
        nyaaClient.Fetch(Query(terms, trustedOnly), token)
            .Bind(document => ToResults(document, binding))
            .WithOperationName(nameof(Search));

    private static NyaaQuery Query(string terms, bool trustedOnly)
    {
        var query = NyaaQuery.Create().Search(terms);
        return trustedOnly ? query.TrustedOnly() : query;
    }

    private static Result<ReleaseSearchResults> ToResults(XDocument document, CurrentBinding binding) =>
        NyaaFeed.Parse(document)
            .AddLogOnSuccess(feed => feed.LogErrors)
            .Map(feed => new ReleaseSearchResults(Candidates(feed.Value, binding), NyaaFeed.IsFull(document)));

    private static ImmutableArray<ReleaseCandidate> Candidates(ImmutableArray<NyaaItem> items, CurrentBinding binding) =>
    [
        ..items
            .SelectMany(item => NyaaReleaseTitle.Parse(item.Title).MatchToValue(
                release => new[] {(Item: item, Release: release)},
                _ => []))
            .GroupBy(found => (found.Release.Group, found.Release.Title, found.Release.Variant))
            .Select(release => new ReleaseCandidate(
                release.Key.Group,
                release.Key.Title,
                release.Key.Variant,
                release.All(found => found.Item.Trusted),
                [
                    ..release.Select(found => found.Release.Episode)
                        .OrderBy(episode => episode is SpecialEpisode)
                        .ThenBy(EpisodeOrder)
                        .Select(episode => episode.Label)
                        .Distinct()
                ],
                release.Max(found => found.Item.Published),
                binding is ActiveBinding active
                && active.Group == release.Key.Group
                && active.ReleaseTitle == release.Key.Title
                && active.Variant == release.Key.Variant))
            .OrderByDescending(candidate => candidate.Newest)
    ];

    private static decimal EpisodeOrder(NyaaEpisode episode) => episode switch
    {
        RegularEpisode regular => regular.Number,
        SpecialEpisode special => special.Number,
        _ => throw new ArgumentOutOfRangeException(nameof(episode), episode, null)
    };
}
