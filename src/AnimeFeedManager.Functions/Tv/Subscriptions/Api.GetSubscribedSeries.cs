using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class GetSubscribedSeries
{
    private readonly IGetTvSubscriptions _tvSubscriptions;
    private readonly ILogger<GetInterestedSeries> _logger;

    public GetSubscribedSeries(IGetTvSubscriptions tvSubscriptions, ILoggerFactory loggerFactory)
    {
        _tvSubscriptions = tvSubscriptions;
        _logger = loggerFactory.CreateLogger<GetInterestedSeries>();
    }

    [Function("GetSubscribedSeries")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await req
            .CheckAuthorization()
            .BindAsync(_ => UserIdValidator.Validate(subscriber).ValidationToEither())
            .BindAsync(userId => _tvSubscriptions.GetUserSubscriptions(userId, default))
            .MapAsync(collection => collection.Series.Select(s => s.ToString()))
            .ToResponse(req, _logger);
    }
}