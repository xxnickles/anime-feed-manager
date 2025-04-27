using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tv.Library.Messages;

public sealed record TvSeriesToUpdate(AnimeInfoStorage Series) :  DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-library-series-update";
}