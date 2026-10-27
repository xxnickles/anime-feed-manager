using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tv.FeedManager.Matching;

public static class NyaaMatch
{
    private static readonly NyaaCatalogStorage[] NoRows = [];

    /// <summary>Exact group, release title and variant; a trusted binding accepts only trusted items.</summary>
    public static bool Matches(NyaaBindingStorage binding, NyaaItem item, NyaaRelease release) =>
        release.Group == binding.Group
        && release.Title == binding.ReleaseTitle
        && release.Variant == binding.Variant
        && (item.Trusted || !binding.Trusted);

    /// <summary>One catalog row per matching item and row key; a title that is not a single release never matches.</summary>
    public static ImmutableArray<NyaaCatalogStorage> CatalogRows(
        NyaaBindingStorage binding,
        string seriesId,
        IEnumerable<NyaaItem> items) =>
        items.SelectMany(item => NyaaReleaseTitle.Parse(item.Title).MatchToValue(
                release => Matches(binding, item, release) ? [release.ToCatalogRow(item, seriesId)] : NoRows,
                _ => NoRows))
            .DistinctBy(row => row.RowKey)
            .ToImmutableArray();
}
