namespace AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;

/// <summary>One RSS item from the Nyaa feed — a single torrent release.</summary>
public sealed record NyaaEntry(string Title, string Link, string Guid, DateTimeOffset PublishedAt);
