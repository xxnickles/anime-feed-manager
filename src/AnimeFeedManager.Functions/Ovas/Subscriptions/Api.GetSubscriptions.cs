using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Subscriptions;

public class GetSubscriptions
{
    private readonly IGetOvasSubscriptions _ovasSubscriptions;
    private readonly ILogger<GetSubscriptions> _logger;

    public GetSubscriptions(IGetOvasSubscriptions ovasSubscriptions, ILoggerFactory loggerFactory)
    {
        _ovasSubscriptions = ovasSubscriptions;
        _logger = loggerFactory.CreateLogger<GetSubscriptions>();
    }

    [Function("GetOvasSubscriptions")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ovas/subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await UserIdValidator.Validate(subscriber).ValidationToEither()
            .BindAsync(id => _ovasSubscriptions.GetSubscriptions(id, default))
            .ToResponse(req, _logger);
    }
}