using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Subscriptions;

public class GetSubscriptions(IGetOvasSubscriptions ovasSubscriptions, ILoggerFactory loggerFactory)
{
    private readonly ILogger<GetSubscriptions> _logger = loggerFactory.CreateLogger<GetSubscriptions>();

    [Function("GetOvasSubscriptions")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ovas/subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await UserId.Parse(subscriber)
            .BindAsync(id => ovasSubscriptions.GetSubscriptions(id, default))
            .ToResponse(req, _logger);
    }
}