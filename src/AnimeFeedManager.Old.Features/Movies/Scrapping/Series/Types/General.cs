using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types;

public readonly record struct MoviesCollection(
    SeasonInformation SeasonInformation,
    ImmutableList<MovieStorage> SeriesList, 
    ImmutableList<DownloadImageEvent> Images);