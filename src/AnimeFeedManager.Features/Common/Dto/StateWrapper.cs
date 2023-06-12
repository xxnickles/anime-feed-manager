namespace AnimeFeedManager.Features.Common.Dto;

public record StateWrapper<T>(string Id, T Payload);