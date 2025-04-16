using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Movies.Library.Types;

public record MovieLibrary(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime? AirDate,
    SeriesFeedLinks[] Links) : BaseAnime(Id, Season, Title, Synopsis, ImageUrl);

