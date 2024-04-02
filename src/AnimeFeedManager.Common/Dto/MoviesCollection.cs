namespace AnimeFeedManager.Common.Dto;

public record NullMovie() : BaseMovie(string.Empty, string.Empty, string.Empty, string.Empty, null);

public sealed record NotAvailableMovie(string Id, string Season, string Title, string Synopsis, string? ImageUrl)
    : BaseMovie(Id, Season, Title, Synopsis, ImageUrl);

public sealed record AvailableMovie(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate)
    : BaseMovie(Id, Season, Title, Synopsis, ImageUrl);

public abstract record MoviesForUser(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin)
    : BaseMovie(Id, Season, Title, Synopsis, ImageUrl);

public record UnsubscribedMovie(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin) : MoviesForUser(Id, Season, Title, Synopsis, ImageUrl, AirDate, UserId, IsAdmin);
    
public record SubscribedMovie(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime AirDate,
    UserId UserId,
    bool IsAdmin) : MoviesForUser(Id, Season, Title, Synopsis, ImageUrl, AirDate, UserId, IsAdmin);