using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Ovas.Library.Types;

public record OvaLibrary(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime? AirDate,
    SeriesFeedLinks[] Links) : BaseAnime(Id, Season, Title, Synopsis, ImageUrl);

