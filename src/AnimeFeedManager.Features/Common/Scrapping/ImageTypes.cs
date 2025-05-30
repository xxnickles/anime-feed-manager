namespace AnimeFeedManager.Features.Common.Scrapping;

public abstract record ImageInformation;

internal sealed record NoImage : ImageInformation;

internal sealed record AlreadyExistInSystem : ImageInformation;

internal sealed record ScrappedImageUrl(Uri Url) : ImageInformation;

