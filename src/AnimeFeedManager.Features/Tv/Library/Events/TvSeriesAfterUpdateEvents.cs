namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record FeedTitlesUpdated(SeriesSeason Season, string[] FeedTitles) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "feed-be-updated-events";
    public override BinaryData ToBinaryData()
    {
       return BinaryData.FromObjectAsJson(this, FeedTitlesUpdatedContext.Default.FeedTitlesUpdated);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(FeedTitlesUpdated))]
public partial class FeedTitlesUpdatedContext : JsonSerializerContext;

public sealed record SeriesFeedUpdated(string SeriesId, string SeriesFeed) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "series-feed-updated-events";
    public override BinaryData ToBinaryData()
    {
       return BinaryData.FromObjectAsJson(this, SeriesFeedUpdatedContext.Default.SeriesFeedUpdated);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(SeriesFeedUpdated))]
public partial class SeriesFeedUpdatedContext : JsonSerializerContext;
