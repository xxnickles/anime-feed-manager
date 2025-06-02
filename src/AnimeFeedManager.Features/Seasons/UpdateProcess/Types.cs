namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public abstract record SeasonStorageData;

public sealed record NoUpdateRequired : SeasonStorageData;
public sealed record NoMatch : SeasonStorageData;

public sealed record NewSeason(SeasonStorage Season) : SeasonStorageData;

public sealed record ExistentSeason(SeasonStorage Season) : SeasonStorageData;

public sealed record LatestSeason(SeasonStorage Season) : SeasonStorageData;

public sealed record SeasonUpdateData(
    SeriesSeason SeasonToUpdate, 
    SeasonStorageData SeasonData, 
    SeasonStorageData LatestSeasonData);

