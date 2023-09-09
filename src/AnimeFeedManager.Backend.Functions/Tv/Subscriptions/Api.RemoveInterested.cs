using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Tv.Subscriptions;

public class RemoveInterested
{
    private readonly IRemoveInterestedSeries _interestedSeriesRemover;
    private readonly ILogger<RemoveInterested> _logger;

    public RemoveInterested(
        IRemoveInterestedSeries interestedSeriesRemover,
        ILoggerFactory loggerFactory)
    {
        _interestedSeriesRemover = interestedSeriesRemover;
        _logger = loggerFactory.CreateLogger<RemoveInterested>();
    }

    [Function("RemoveTvInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/removeInterested")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, SimpleTvSubscriptionContext.Default.SimpleTvSubscription);
        ArgumentNullException.ThrowIfNull(payload);

        return await Utils.Validate(payload)
            .BindAsync(param => _interestedSeriesRemover.Remove(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}