namespace AnimeFeedManager.Features.Scrapping.Types;

public record FeedData(string Title, string Url);

public record EpisodeData(
    string EpisodeNumber,
    string MagnetLink,
    string TorrentLink,
    bool IsNew);

public record DailySeriesFeed(
    string Title,
    string Url,
    ImmutableList<EpisodeData> Episodes);
    
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DailySeriesFeed[]))]
public partial class DailyFeedContext : JsonSerializerContext;