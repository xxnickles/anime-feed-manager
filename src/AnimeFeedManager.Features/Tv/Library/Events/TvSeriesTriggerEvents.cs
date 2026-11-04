using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record UpdateTvSeriesEvent(SeasonParameters? SeasonParameters = null) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "update-tv-series";
    
    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.UpdateTvSeriesEvent);
    }
}

