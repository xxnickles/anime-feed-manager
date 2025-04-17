using AnimeFeedManager.Old.Common.Domain.Types;

namespace AnimeFeedManager.Old.Common.Dto;

public record NullTvAnime() : BaseTvAnime(string.Empty, string.Empty, string.Empty, string.Empty, null);

public sealed record FeedData(string Status, string? Title);

public sealed record FeedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    SeriesStatus SeriesStatus,
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
    bool IsAdmin,
    SeriesStatus SeriesStatus)
    : BaseTvAnime(Id, Season, Title, Synopsis, ImageUrl);

public sealed record NotAvailableAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    UserId UserId,
    bool IsAdmin,
    SeriesStatus SeriesStatus) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin, SeriesStatus);

public sealed record InterestedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    UserId UserId,
    bool IsAdmin,
    SeriesStatus SeriesStatus) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin,SeriesStatus);

public sealed record UnSubscribedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    string FeedId,
    UserId UserId,
    bool IsAdmin,
    SeriesStatus SeriesStatus) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin, SeriesStatus);

public sealed record SubscribedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    string FeedId,
    UserId UserId,
    bool IsAdmin,
    SeriesStatus SeriesStatus) : AnimeForUser(Id, Season, Title, Synopsis, ImageUrl, UserId, IsAdmin, SeriesStatus);