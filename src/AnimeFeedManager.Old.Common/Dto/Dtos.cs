namespace AnimeFeedManager.Old.Common.Dto;

public record BasicSeason(string Season, ushort Year);

public record ShorSeriesLatestSeason(bool KeeepFeed);

public record ShortSeriesSeason(string Season, ushort Year, bool KeeepFeed);

public record SimpleSeasonInfo(string Season, int Year, bool IsLatest);

public record NullSimpleSeasonInfo() : SimpleSeasonInfo(string.Empty, 0, false);