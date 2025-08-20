using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record UpdateTvSeriesEvent(SeasonParameters? SeasonParameters = null) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "update-tv-series";
    
    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, UpdateTvSeriesEventContext.Default.UpdateTvSeriesEvent);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(UpdateTvSeriesEvent))]
public partial class UpdateTvSeriesEventContext : JsonSerializerContext;

public sealed record UpdateLatestFeedTitlesEvent(bool Update = true) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "update-latest-feed-titles";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, UpdateLatestFeedTitlesEventContext.Default.UpdateLatestFeedTitlesEvent);;
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(UpdateLatestFeedTitlesEvent))]
public partial class UpdateLatestFeedTitlesEventContext : JsonSerializerContext;