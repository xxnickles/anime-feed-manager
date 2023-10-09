namespace AnimeFeedManager.Common.Domain.Types;

public record struct ImageInformation(string Id, string Name, string? Link, SeasonInformation SeasonInfo);

public record struct AnimeInfo(
    string Id,
    string Title,
    string Synopsis,
    string FeedTitle,
    SeasonInformation SeasonInformation,
    DateTime? Date,
    bool Completed);

public record struct ShortAnimeInfo(
    string Id,
    string Title,
    string Synopsis,
    SeasonInformation SeasonInformation,
    DateTime? Date);

public record Ovas(ImmutableList<ShortAnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);

public record Movies(ImmutableList<ShortAnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);