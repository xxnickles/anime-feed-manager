using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.FeedTitlesUpdateProcess;

public static class FeedTitlesUpdate
{
    public static Task<Result<FeedTitlesUpdateResult>> StoreTitles(FeedTitleUpdateData data, FeedTitlesUpdater updater,
        CancellationToken token) =>
        updater(data.FeedTitles.MapToStorage(), token)
            .Map(_ => new FeedTitlesUpdateResult(data.Season, data.FeedTitles.Length));

    private static FeedTitlesStorage MapToStorage(this string[] feedTitles) => new()
    {
        Payload = JsonSerializer.Serialize(feedTitles)
    };
}