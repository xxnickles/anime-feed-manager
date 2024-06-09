using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using SimpleTvSubscriptionContext = AnimeFeedManager.Common.Dto.SimpleTvSubscriptionContext;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class RemoveInterested(
    IRemoveInterestedSeries interestedSeriesRemover,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<RemoveInterested> _logger = loggerFactory.CreateLogger<RemoveInterested>();

    [Function("RemoveTvInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/removeInterested")]
        HttpRequestData req, CancellationToken token)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, SimpleTvSubscriptionContext.Default.SimpleTvSubscription, token);
        ArgumentNullException.ThrowIfNull(payload);

        return await Utils.Validate(payload)
            .BindAsync(param => interestedSeriesRemover.Remove(param.UserId, param.Series, token))
            .ToResponse(req, _logger);
    }
}