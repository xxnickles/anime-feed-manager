﻿using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Series.Types;

public readonly record struct OvasCollection(
    SeasonInformation SeasonInformation,
    ImmutableList<OvaStorage> SeriesList, 
    ImmutableList<DownloadImageEvent> Images);