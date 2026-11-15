namespace AnimeFeedManager.Features.Images;

public sealed record ImageNotStored(string FileName, Exception Exception) : DomainError(Exception.Message)
{
    public override Action<ILogger> LogAction() => logger =>
        logger.LogError(Exception, "An error occurred when processing image {image.name}", FileName);
}
