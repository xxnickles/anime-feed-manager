using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class AddInterested(
    IAddInterested addInterested,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<AddInterested> _logger = loggerFactory.CreateLogger<AddInterested>();

    [Function("AddTvInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "tv/interested")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body,
                InterestedTvSubscriptionContext.Default.InterestedTvSubscription);
        ArgumentNullException.ThrowIfNull(payload);

        return await Utils.Validate(payload)
            .BindAsync(param => addInterested.Add(param.UserId, param.SeriesId, param.Series, default))
            .ToResponse(req, _logger);
    }
}