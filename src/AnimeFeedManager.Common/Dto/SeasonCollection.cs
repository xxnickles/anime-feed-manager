using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.Dto;

public abstract record BaseAnime(string Id, string Season, string Title, string Synopsis, string? Url);

public record NullAnime() : BaseAnime(string.Empty, string.Empty, string.Empty, string.Empty, null);

public sealed record FeedData(bool Available, string Status, string? Title);

public sealed record FeedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? Url,
    FeedData FeedInformation) : BaseAnime(Id, Season, Title, Synopsis, Url);

public sealed record CompletedAnime(string Id, string Season, string Title, string Synopsis, string? Url)
    : BaseAnime(Id, Season, Title, Synopsis, Url);

public abstract record AnimeForUser(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? Url,
    UserId UserId,
    bool IsAdmin)
    : BaseAnime(Id, Season, Title, Synopsis, Url);

public sealed record NotAvailableAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? Url,
    string AnimeTitle,
    UserId UserId,
    bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, Url, UserId, IsAdmin);

public sealed record InterestedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? Url,
    string AnimeTitle,
    UserId UserId,
    bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, Url, UserId, IsAdmin);

public sealed record UnSubscribedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? Url,
    string FeedId,
    UserId UserId,
    bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, Url, UserId, IsAdmin);

public sealed record SubscribedAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? Url,
    string FeedId,
    UserId UserId, bool IsAdmin) : AnimeForUser(Id, Season, Title, Synopsis, Url, UserId, IsAdmin);

public record SeasonCollection(ushort Year, string Season, FeedAnime[] Animes);

public record EmptySeasonCollection() : SeasonCollection(0, string.Empty, System.Array.Empty<FeedAnime>());

[JsonSerializable(typeof(SeasonCollection))]
[JsonSerializable(typeof(FeedAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SeasonCollectionContext : JsonSerializerContext
{
}

public sealed record SimpleAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? Url,
    DateTime? AirDate) : BaseAnime(Id, Season, Title, Synopsis, Url);

public record ShortSeasonCollection(ushort Year, string Season, SimpleAnime[] Animes);

public record EmptyShortSeasonCollection() : ShortSeasonCollection(0, string.Empty, System.Array.Empty<SimpleAnime>());

[JsonSerializable(typeof(ShortSeasonCollection))]
[JsonSerializable(typeof(SimpleAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ShortSeasonCollectionContext : JsonSerializerContext
{
}