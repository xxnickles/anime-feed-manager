using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Dto;

namespace AnimeFeedManager.Old.Features.Ovas.Library.Types;

public record OvaLibrary(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime? AirDate,
    SeriesFeedLinks[] Links) : BaseAnime(Id, Season, Title, Synopsis, ImageUrl);

