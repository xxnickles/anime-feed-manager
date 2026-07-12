using System.Globalization;
using System.Xml.Linq;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Feeds.Sources.Nyaa;

/// <summary>
/// Thin HTTP boundary over the Nyaa RSS feed. Fetches the current feed for the configured
/// category/filter as one flat snapshot (newest first) — no per-series querying; matching
/// against the library happens downstream.
/// </summary>
public interface INyaaClient
{
    Task<Result<ImmutableArray<NyaaEntry>>> GetLatest(CancellationToken token = default);
}

internal sealed class NyaaClient(HttpClient httpClient, IOptions<NyaaOptions> options) : INyaaClient
{
    public async Task<Result<ImmutableArray<NyaaEntry>>> GetLatest(CancellationToken token = default)
    {
        var opts = options.Value;
        try
        {
            using var response = await httpClient.GetAsync($"?page=rss&c={opts.Category}&f={opts.Filter}", token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, token);

            return ParseEntries(document);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static ImmutableArray<NyaaEntry> ParseEntries(XDocument document) =>
        [..document.Descendants("item").Select(ParseEntry).OfType<NyaaEntry>()];

    // Malformed items (missing a field, an unparsable pubDate) are dropped rather than failing
    // the whole fetch — Nyaa's feed is well-formed in practice, and one bad item shouldn't cost
    // the run every other entry.
    private static NyaaEntry? ParseEntry(XElement item)
    {
        var title = item.Element("title")?.Value;
        var link = item.Element("link")?.Value;
        var guid = item.Element("guid")?.Value;
        var pubDate = item.Element("pubDate")?.Value;

        if (title is null || link is null || guid is null || pubDate is null)
            return null;

        return DateTimeOffset.TryParse(pubDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var publishedAt)
            ? new NyaaEntry(title, link, guid, publishedAt)
            : null;
    }
}
