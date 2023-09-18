using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Types;

public readonly record struct OvasCollection(ImmutableList<OvaStorage> SeriesList, ImmutableList<DownloadImageEvent> Images);