using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types;

public readonly record struct OvasCollection(
    SeasonInformation SeasonInformation,
    ImmutableList<OvaStorage> SeriesList, 
    ImmutableList<DownloadImageEvent> Images);