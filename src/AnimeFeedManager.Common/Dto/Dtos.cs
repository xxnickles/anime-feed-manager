namespace AnimeFeedManager.Common.Dto;

public record SeasonInfoDto(string Season, int Year);

public record NullSeasonInfo() : SeasonInfoDto(string.Empty, 0);
public record SubscriptionDto(string UserId, string Series);

public record UserDto(string UserId, string Email);