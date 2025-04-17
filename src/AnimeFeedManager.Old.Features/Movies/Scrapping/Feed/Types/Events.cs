using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Dto;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Movies.Scrapping.Feed.Types;

public record StartScrapMoviesFeed(FeedType Type) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "movies-feed-process";
}

public record ScrapMoviesSeasonFeed(BasicSeason SeasonInformation) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "movies-season-feed-process";
}

public record UpdateMovieFeed(MovieStorage Series, ImmutableList<SeriesFeedLinks> Links) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "movies-season-feed-update";
}
