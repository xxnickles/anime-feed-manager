namespace AnimeFeedManager.Common.Dto
{
    public record StateWrapper<T>(string Id, T Payload);
}