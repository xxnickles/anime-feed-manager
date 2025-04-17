using AnimeFeedManager.Old.Common.Domain.Types;

namespace AnimeFeedManager.Old.Common.Dto;

public record NullMovie() : BaseMovie(string.Empty, string.Empty, string.Empty, string.Empty, null);

public sealed record NotAvailableMovie(string Id, string Season, string Title, string Synopsis, string? ImageUrl)
    : BaseMovie(Id, Season, Title, Synopsis, ImageUrl);

public abstract record BaseAvailableMovie(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    SeriesFeedLinks[] Links) : BaseMovie(Id, Season, Title, Synopsis, ImageUrl);

public sealed record AvailableMovie(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    SeriesFeedLinks[] Links)
    : BaseAvailableMovie(Id, Season, Title, Synopsis, ImageUrl, AirDate, Links);

public abstract record MovieForUser(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin,
    SeriesFeedLinks[] Links)
    : BaseAvailableMovie(Id, Season, Title, Synopsis, ImageUrl, AirDate, Links);

public record UnsubscribedMovie(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin,
    SeriesFeedLinks[] Links) : MovieForUser(Id, Season, Title, Synopsis, ImageUrl, AirDate, UserId, IsAdmin, Links);

public record SubscribedMovie(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin,
    SeriesFeedLinks[] Links) : MovieForUser(Id, Season, Title, Synopsis, ImageUrl, AirDate, UserId, IsAdmin, Links);