namespace AnimeFeedManager.Common.Dto;

public record NullTvAnime() : BaseTvAnime(string.Empty, string.Empty, string.Empty, string.Empty, null);

public sealed record FeedData(bool Available, string Status, string? Title);

public sealed record FeedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    FeedData FeedInformation) : BaseTvAnime(Id, Season, Title, Synopsis, ImageUrl);

public sealed record CompletedAnime(string Id, string Season, string Title, string Synopsis, string? ImageUrl)
    : BaseTvAnime(Id, Season, Title, Synopsis, ImageUrl);

public abstract record AnimeForUser(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    UserId UserId,
    bool IsAdmin)
    : BaseTvAnime(Id, Season, Title, Synopsis, ImageUrl);

public sealed record NotAvailableAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    UserId UserId,
    bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin);

public sealed record InterestedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    UserId UserId,
    bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin);

public sealed record UnSubscribedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    string FeedId,
    UserId UserId,
    bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin);

public sealed record SubscribedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    string FeedId,
    UserId UserId,
    bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin);