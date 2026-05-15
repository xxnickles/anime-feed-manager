namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanTitle(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title);
