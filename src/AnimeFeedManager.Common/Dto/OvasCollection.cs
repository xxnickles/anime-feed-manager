using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Common.Dto;

public record NullOva() : BaseOva(string.Empty, string.Empty, string.Empty, string.Empty, null);

public sealed record NotAvailableOva(string Id, string Season, string Title, string Synopsis, string? ImageUrl)
    : BaseOva(Id, Season, Title, Synopsis, ImageUrl);

public sealed record AvailableOva(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    SeriesFeedLinks[] Links)
    : BaseOva(Id, Season, Title, Synopsis, ImageUrl);

public abstract record OvaForUser(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin,
    SeriesFeedLinks[] Links)
    : BaseOva(Id, Season, Title, Synopsis, ImageUrl);

public record UnsubscribedOva(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin,
    SeriesFeedLinks[] Links) : OvaForUser(Id, Season, Title, Synopsis, ImageUrl, AirDate, UserId, IsAdmin, Links);

public record SubscribedOva(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin,
    SeriesFeedLinks[] Links) : OvaForUser(Id, Season, Title, Synopsis, ImageUrl, AirDate, UserId, IsAdmin,Links);