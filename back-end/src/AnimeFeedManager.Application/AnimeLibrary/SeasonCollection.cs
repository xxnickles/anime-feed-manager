using System.Collections.Immutable;
using LanguageExt;

namespace AnimeFeedManager.Application.AnimeLibrary
{
    public sealed class FeedInfo : Record<FeedInfo>
    {
        public bool Available { get; }
        public string? Title { get; }
        public bool Completed { get; }

        public FeedInfo(bool available, bool completed, string? title)
        {
            Available = available;
            Completed = completed;
            Title = title;
        }
    }

    public sealed class SimpleAnime : Record<SimpleAnime>
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

    public sealed class SeasonCollection : Record<SeasonCollection>
    {
        public string Season { get; }
        public ushort Year { get; }
        public ImmutableList<SimpleAnime> Animes { get; }

        public SeasonCollection(ushort year, string season, ImmutableList<SimpleAnime> animes)
        {
            Year = year;
            Animes = animes;
            Season = season;
        }
    }
}
