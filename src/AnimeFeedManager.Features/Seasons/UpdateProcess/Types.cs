namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public abstract record SeasonStorageData;

public sealed record NoUpdateRequired : SeasonStorageData;
public sealed record NoMatch : SeasonStorageData;

public sealed record NewSeason(SeasonStorage Season) : SeasonStorageData;

public sealed record ExistentSeason(SeasonStorage Season) : SeasonStorageData;

public sealed record ReplaceLatestSeason(SeasonStorage Season) : SeasonStorageData;

public sealed record CurrentLatestSeason(SeasonStorage Season) : SeasonStorageData;

/// <summary>
/// The newest known season, stood in for a featured one when nothing carries the flag. Read paths
/// treat it as featured; the promotion path must not, or it would demote the season it just promoted.
/// </summary>
public sealed record FallbackLatestSeason(SeasonStorage Season) : SeasonStorageData;

public sealed record SeasonUpdateData(
    SeriesSeason SeasonToUpdate, 
    SeasonStorageData SeasonData, 
    SeasonStorageData CurrentLatestSeasonData);

