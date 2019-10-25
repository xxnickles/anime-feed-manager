using System.Collections.Immutable;
using LanguageExt;

namespace AnimeFeedManager.Application.AnimeLibrary
{
    public sealed class SimpleAnime : Record<SimpleAnime>
    {
        public string Id { get; }
        public string? ImageUrl { get;  }
        public string Title { get; }
        public string? Synopsis { get; }
        public bool HasAvailableFeed { get; }
        public string? FeedTitle { get; }

        public SimpleAnime(string id, string? imageUrl, string title, string? synopsis, bool hasAvailableFeed, string? feedTitle)
        {
            Id = id;
            ImageUrl = imageUrl;
            Title = title;
            Synopsis = synopsis ?? "Not Available";
            HasAvailableFeed = hasAvailableFeed;
            FeedTitle = feedTitle;
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
