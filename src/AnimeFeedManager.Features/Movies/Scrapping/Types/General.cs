using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.Types
{
    public readonly record struct MoviesCollection(ImmutableList<MovieStorage> SeriesList, ImmutableList<DownloadImageEvent> Images);
}