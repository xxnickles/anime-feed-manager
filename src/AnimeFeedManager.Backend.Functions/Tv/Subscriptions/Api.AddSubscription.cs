using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Tv.Subscriptions;

public class AddSubscription
{
    private readonly IAddTvSubscription _tvSubscriptionAdder;
    private readonly ILogger<AddSubscription> _logger;

    public AddSubscription(
        IAddTvSubscription tvSubscriptionAdder,
        ILoggerFactory loggerFactory)
    {
        _tvSubscriptionAdder = tvSubscriptionAdder;
        _logger = loggerFactory.CreateLogger<AddSubscription>();
    }

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
            .BindAsync(param => _tvSubscriptionAdder.Subscribe(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}