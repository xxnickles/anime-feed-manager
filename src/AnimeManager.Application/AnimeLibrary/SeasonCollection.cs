using System.Collections.Immutable;
using LanguageExt;

namespace AnimeFeedManager.Application.AnimeLibrary
{
    public sealed class FeedInfo : Record<FeedInfo>
    {
        public bool Available { get; }
        public string? Title { get; }

        public FeedInfo(bool available, string? title)
        {
            Available = available;
            Title = title;
        }

       
    }

    public sealed class SimpleAnime : Record<SimpleAnime>
    {
        public string Id { get; }
        public string? ImageUrl { get;  }
        public string Title { get; }
        public string? Synopsis { get; }
        public FeedInfo FeedInformation { get; }

        public SimpleAnime(string id, string? imageUrl, string title, string? synopsis, bool hasAvailableFeed, string? feedTitle)
        {
            Id = id;
            ImageUrl = imageUrl;
            Title = title;
            Synopsis = synopsis ?? "Not Available";
            FeedInformation = new FeedInfo(hasAvailableFeed, feedTitle);
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
