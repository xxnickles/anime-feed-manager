namespace AnimeFeedManager.Features.Scrapping.Messages;

public enum ScrapType
{
    Latest,
    BySeason
}

public sealed record SeasonToScrap(string Season, ushort Year);

public sealed record ScrapLibrary(
    SeriesType SeriesType,
    SeasonToScrap? Season,
    ScrapType ScrapType,
    bool KeepFeed = false) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "library-scrap-events";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, ScrapLibraryContext.Default.ScrapLibrary);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ScrapLibrary))]
public partial class ScrapLibraryContext : JsonSerializerContext;