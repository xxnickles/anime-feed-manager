using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using SimpleTvSubscriptionContext = AnimeFeedManager.Common.Dto.SimpleTvSubscriptionContext;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class Unsubscribe
{
    private readonly IRemoveTvSubscription _tvUnsubscriber;
    private readonly ILogger<Unsubscribe> _logger;

    public Unsubscribe(IRemoveTvSubscription tvUnsubscriber, ILoggerFactory loggerFactory)
    {
        _tvUnsubscriber = tvUnsubscriber;
        _logger = loggerFactory.CreateLogger<Unsubscribe>();
    }

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
            .BindAsync(param => _tvUnsubscriber.Unsubscribe(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}