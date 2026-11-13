namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

public sealed record UserIsAnonymous() : DomainError("User can not be anonymous")
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}
