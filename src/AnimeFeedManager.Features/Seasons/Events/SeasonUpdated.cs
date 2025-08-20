namespace AnimeFeedManager.Features.Seasons.Events;

public sealed record SeasonUpdated(SeriesSeason Season) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "season-updated-events";
    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, SeasonUpdatedContext.Default.SeasonUpdated);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(SeasonUpdated))]
public partial class SeasonUpdatedContext : JsonSerializerContext;

