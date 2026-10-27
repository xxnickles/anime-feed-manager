using System.Globalization;
using System.Xml.Linq;

namespace AnimeFeedManager.Features.Scrapping.Nyaa;

/// <param name="InfoHash">Lowercase 40-character hex.</param>
/// <param name="Size">As displayed by nyaa, e.g. <c>574.7 MiB</c>.</param>
/// <param name="Category">Nyaa category code, e.g. <c>1_2</c>.</param>
public sealed record NyaaItem(
    string Title,
    string Magnet,
    string Torrent,
    string InfoHash,
    DateTimeOffset Published,
    string Size,
    bool Trusted,
    bool Remake,
    string Category);

public static class NyaaFeed
{
    private static readonly XNamespace Nyaa = "https://nyaa.si/xmlns/nyaa";
    private const string PubDateFormat = "ddd, dd MMM yyyy HH:mm:ss zzz";

    // Nyaa answers at most 75 items, newest first, and ignores paging.
    private const int MaxItems = 75;

    /// <summary>Whether the feed hit nyaa's 75-item cap, so older releases may be missing.</summary>
    public static bool IsFull(XDocument document) =>
        document.Root?.Element("channel")?.Elements("item").Count() >= MaxItems;

    /// <summary>Parses an RSS feed requested with <c>m=true</c>; an item with any field missing or invalid is skipped.</summary>
    public static Result<BulkResult<ImmutableArray<NyaaItem>>> Parse(XDocument document) =>
        document.Root?.Element("channel") is { } channel
            ? channel.Elements("item")
                .Select(ParseItem)
                .Flatten(items => items.ToImmutableArray())
            : Error.Create("Nyaa response is not an RSS feed");

    private static Result<NyaaItem> ParseItem(XElement item) =>
        Required(item, "title")
            .And(Magnet(item))
            .And(Torrent(item))
            .And(InfoHash(item))
            .And(Published(item))
            .And(Required(item, Nyaa + "size"))
            .And(YesNo(item, Nyaa + "trusted"))
            .And(YesNo(item, Nyaa + "remake"))
            .And(Required(item, Nyaa + "categoryId"))
            .Map(fields => new NyaaItem(
                Title: fields.Item1,
                Magnet: fields.Item2,
                Torrent: fields.Item3,
                InfoHash: fields.Item4,
                Published: fields.Item5,
                Size: fields.Item6,
                Trusted: fields.Item7,
                Remake: fields.Item8,
                Category: fields.Item9));

    private static Validation<string> Required(XElement item, XName name) =>
        Value(item, name) is { } value ? Validation<string>.Valid(value) : Missing<string>(name);

    private static Validation<string> Magnet(XElement item) =>
        Value(item, "link") switch
        {
            null => Missing<string>("link"),
            var link when link.StartsWith("magnet:?", StringComparison.Ordinal) => Validation<string>.Valid(link),
            var link => Invalid<string>("link", $"'{link}' is not a magnet link")
        };

    // guid is the view page, https://nyaa.si/view/{id}; the torrent is served from /download/{id}.torrent.
    private static Validation<string> Torrent(XElement item) =>
        Value(item, "guid") switch
        {
            null => Missing<string>("guid"),
            var guid when Uri.TryCreate(guid, UriKind.Absolute, out var view)
                          && view.Segments is [_, "view/", var id]
                          && id.All(char.IsAsciiDigit) =>
                Validation<string>.Valid(new Uri(view, $"/download/{id}.torrent").ToString()),
            var guid => Invalid<string>("guid", $"'{guid}' is not a nyaa view URL")
        };

    private static Validation<string> InfoHash(XElement item) =>
        Value(item, Nyaa + "infoHash")?.ToLowerInvariant() switch
        {
            null => Missing<string>(Nyaa + "infoHash"),
            { Length: 40 } hash when hash.All(char.IsAsciiHexDigitLower) => Validation<string>.Valid(hash),
            var hash => Invalid<string>(Nyaa + "infoHash", $"'{hash}' is not a 40-character hex info hash")
        };

    private static Validation<DateTimeOffset> Published(XElement item) =>
        Value(item, "pubDate") switch
        {
            null => Missing<DateTimeOffset>("pubDate"),
            var date when DateTimeOffset.TryParseExact(date, PubDateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var published) => Validation<DateTimeOffset>.Valid(published),
            var date => Invalid<DateTimeOffset>("pubDate", $"'{date}' is not an RFC 822 date")
        };

    private static Validation<bool> YesNo(XElement item, XName name) =>
        Value(item, name) switch
        {
            null => Missing<bool>(name),
            "Yes" => Validation<bool>.Valid(true),
            "No" => Validation<bool>.Valid(false),
            var flag => Invalid<bool>(name, $"'{flag}' is not Yes/No")
        };

    private static string? Value(XElement item, XName name) =>
        item.Element(name)?.Value.Trim() is { Length: > 0 } value ? value : null;

    private static Validation<T> Missing<T>(XName name) => Invalid<T>(name, "Missing");

    private static Validation<T> Invalid<T>(XName name, string message) =>
        Validation<T>.Invalid(DomainValidationError.Create(name.LocalName, message).ToErrors());
}
