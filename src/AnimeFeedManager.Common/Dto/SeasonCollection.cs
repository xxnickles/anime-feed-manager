namespace AnimeFeedManager.Common.Dto;

public sealed record FeedInfo(bool Available, bool Completed, string? Title);

public sealed record SimpleAnime(string Id, string Title, string Synopsis, string? Url);

public sealed record FeedAnime(string Id, string Title, string Synopsis, string? Url, FeedInfo FeedInformation);

public record SeasonCollection(ushort Year, string Season, FeedAnime[] Animes);

public record ShortSeasonCollection(ushort Year, string Season, SimpleAnime[] Animes);

public record EmptySeasonCollection() : SeasonCollection(0, string.Empty, Array.Empty<FeedAnime>());