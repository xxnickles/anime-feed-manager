namespace AnimeFeedManager.Features.Tv.Library.Events;

/// <summary>
/// Event that represents the update of feed titles for a specific season. It will be used to update the feed titles in the database.
/// </summary>
/// <param name="Season">The season information, including the season type and year, for which the feed titles are being updated.</param>
/// <param name="FeedTitles">An array containing the updated feed titles associated with the specified season.</param>
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

/// <summary>
/// Event that represents updates made to the feed of a specific series. It will be used to change the status of the subscriptions from interested to subscribed.
/// </summary>
/// <param name="SeriesId">The unique identifier for the series being updated.</param>
/// <param name="SeriesFeed">The updated or newly assigned feed information for the series.</param>
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

/// <summary>
/// Event that represents a series that have been completed. Will be used to Expire tv subscriptions.
/// </summary>
/// <param name="Id">All series that have been marked as completed</param>
public sealed record CompletedSeries(string Id) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "completed-series-events";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, CompletedSeriesContext.Default.CompletedSeries);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(CompletedSeries))]
public partial class CompletedSeriesContext : JsonSerializerContext;

/// <summary>
/// Event to verify ongoing series and complete them if they are not in the current feed anymore
/// </summary>
/// <param name="Feed">Feed</param>
public sealed record CompleteOngoingSeries(string[] Feed) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "complete-ongoing-series-events";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, CompleteOngoingSeriesContext.Default.CompleteOngoingSeries);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(CompleteOngoingSeries))]
public partial class CompleteOngoingSeriesContext : JsonSerializerContext;


/// <summary>
/// Event risen when a series is updated to ongoing
/// </summary>
/// <param name="Series"></param>
public sealed record UpdatedToOngoing(string Series, string Feed) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "updated-to-ongoing-events";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, UpdatedToOngoingContext.Default.UpdatedToOngoing);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(UpdatedToOngoing))]
public partial class UpdatedToOngoingContext : JsonSerializerContext;