using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using SimpleTvSubscriptionContext = AnimeFeedManager.Common.Dto.SimpleTvSubscriptionContext;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class Unsubscribe(IRemoveTvSubscription tvUnsubscriber, ILoggerFactory loggerFactory)
{
    private readonly ILogger<Unsubscribe> _logger = loggerFactory.CreateLogger<Unsubscribe>();

    [Function("RemoveTvSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/unsubscribe")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, SimpleTvSubscriptionContext.Default.SimpleTvSubscription);
        ArgumentNullException.ThrowIfNull(payload);
        return await req.CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(param => tvUnsubscriber.Unsubscribe(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}