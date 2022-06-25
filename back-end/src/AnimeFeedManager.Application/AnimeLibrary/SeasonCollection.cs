using System.Collections.Immutable;

namespace AnimeFeedManager.Application.AnimeLibrary;

public sealed record FeedInfo(bool Available, bool Completed, string? Title);

public sealed record SimpleAnime 
{
    public string Id { get; }
    public string Title { get; }
    public string? Url { get; }
    public string? Synopsis { get; }
    public FeedInfo FeedInformation { get; }

    public SimpleAnime(string id, string title, string? synopsis, string? url, bool hasAvailableFeed, string? feedTitle, bool isCompleted)
    {
        Id = id;
        Title = title;
        Url = url;
        Synopsis = synopsis ?? "Not Available";
        FeedInformation = new FeedInfo(hasAvailableFeed, isCompleted, feedTitle);
    }
}

public sealed record SeasonCollection(ushort Year, string Season, ImmutableList<SimpleAnime> Animes);