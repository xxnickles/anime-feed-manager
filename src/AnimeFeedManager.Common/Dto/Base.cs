namespace AnimeFeedManager.Common.Dto;

public abstract record BaseAnime(string Id, string Season, string Title, string Synopsis, string? ImageUrl);

public record NullAnime() : BaseAnime(string.Empty, string.Empty, string.Empty, string.Empty, null);

public abstract record BaseTvAnime(string Id, string Season, string Title, string Synopsis, string? ImageUrl)
    : BaseAnime(Id, Season, Title, Synopsis, ImageUrl);
    
public abstract record BaseOva(string Id, string Season, string Title, string Synopsis, string? ImageUrl)
    : BaseAnime(Id, Season, Title, Synopsis, ImageUrl);

public abstract record BaseMovie(string Id, string Season, string Title, string Synopsis, string? ImageUrl)
    : BaseAnime(Id, Season, Title, Synopsis, ImageUrl);    