namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public sealed record NoUpdateRequired;
public sealed record NoMatch;

public sealed record NewSeason(SeasonStorage Season);

public sealed record ExistentSeason(SeasonStorage Season);

public sealed record ReplaceLatestSeason(SeasonStorage Season);

public sealed record CurrentLatestSeason(SeasonStorage Season);

/// <summary>
/// The newest known season, stood in for a featured one when nothing carries the flag. Read paths
/// treat it as featured; the promotion path must not, or it would demote the season it just promoted.
/// </summary>
public sealed record FallbackLatestSeason(SeasonStorage Season);

public union LatestSeasonRead(CurrentLatestSeason, FallbackLatestSeason, NoMatch);

public union SeasonRead(NoMatch, NoUpdateRequired, ExistentSeason, ReplaceLatestSeason);

public union SeasonPlan(NoUpdateRequired, ExistentSeason, ReplaceLatestSeason, NewSeason);

/// <param name="Latest"><see cref="NoMatch"/> until read; only promotions read it.</param>
public sealed record SeasonUpdateData(
    SeriesSeason SeasonToUpdate,
    SeasonPlan Plan,
    LatestSeasonRead Latest);
