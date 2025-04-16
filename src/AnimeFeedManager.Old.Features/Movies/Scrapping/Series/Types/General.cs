﻿using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.Series.Types;

public readonly record struct MoviesCollection(
    SeasonInformation SeasonInformation,
    ImmutableList<MovieStorage> SeriesList, 
    ImmutableList<DownloadImageEvent> Images);