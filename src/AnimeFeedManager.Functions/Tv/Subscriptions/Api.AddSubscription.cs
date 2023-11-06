using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using SimpleTvSubscriptionContext = AnimeFeedManager.Common.Dto.SimpleTvSubscriptionContext;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class AddSubscription(
    IAddTvSubscription tvSubscriptionAdder,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<AddSubscription> _logger = loggerFactory.CreateLogger<AddSubscription>();

    [Function("AddTvSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "tv/subscriptions")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, SimpleTvSubscriptionContext.Default.SimpleTvSubscription);
        ArgumentNullException.ThrowIfNull(payload);
        return await req.CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(param => tvSubscriptionAdder.Subscribe(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}