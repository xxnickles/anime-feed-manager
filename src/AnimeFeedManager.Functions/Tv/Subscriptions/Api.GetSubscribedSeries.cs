using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class GetSubscribedSeries(IGetTvSubscriptions tvSubscriptions, ILoggerFactory loggerFactory)
{
    private readonly ILogger<GetInterestedSeries> _logger = loggerFactory.CreateLogger<GetInterestedSeries>();

    [Function("GetSubscribedSeries")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await req
            .CheckAuthorization()
            .BindAsync(_ => UserIdValidator.Validate(subscriber).ValidationToEither())
            .BindAsync(userId => tvSubscriptions.GetUserSubscriptions(userId, default))
            .MapAsync(collection => collection.Series.Select(s => s.ToString()))
            .ToResponse(req, _logger);
    }
}