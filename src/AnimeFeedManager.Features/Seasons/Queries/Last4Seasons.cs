namespace AnimeFeedManager.Features.Seasons.Queries;

public static class Last4Seasons
{
    public static Task<Result<ImmutableList<SeriesSeason>>> GetLast4Seasons(Latest4SeasonsGetter seasonsGetter,
        CancellationToken cancellationToken) =>
        seasonsGetter(cancellationToken);
}