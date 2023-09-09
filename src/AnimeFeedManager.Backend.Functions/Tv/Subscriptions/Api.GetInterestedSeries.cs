using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Tv.Subscriptions;

public class GetInterestedSeries
{
    private readonly IGetInterestedSeries _getInterestedSeries;
    private readonly ILogger<GetInterestedSeries> _logger;

    public GetInterestedSeries(IGetInterestedSeries getInterestedSeries, ILoggerFactory loggerFactory)
    {
        _getInterestedSeries = getInterestedSeries;
        _logger = loggerFactory.CreateLogger<GetInterestedSeries>();
    }

    [Function("GetTvInterestedSeries")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tv/interested/{subscriber}")]
        HttpRequestData req,
        string subscriber
    )
    {
        return await req.CheckAuthorization()
            .BindAsync(_ => UserIdValidator.Validate(subscriber).ValidationToEither())
            .BindAsync(user => _getInterestedSeries.Get(user, default))
            .ToResponse(req, _logger);
    }
}