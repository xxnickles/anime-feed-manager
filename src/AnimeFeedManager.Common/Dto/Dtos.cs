namespace AnimeFeedManager.Common.Dto;

public record BasicSeason(string Season, ushort Year);

public abstract record ShorSeriesLatestSeason(bool KeeepFeed);

public abstract record ShorSeriesSeason(string Season, ushort Year, bool KeeepFeed);

public record SimpleSeasonInfo(string Season, int Year, bool IsLatest);

public record NullSimpleSeasonInfo() : SimpleSeasonInfo(string.Empty, 0, false);