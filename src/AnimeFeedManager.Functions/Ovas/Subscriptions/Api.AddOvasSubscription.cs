using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Subscriptions;

public class AddOvasSubscription
{
    private readonly IAddOvasSubscription _ovasSubscription;
    private readonly ILogger<AddOvasSubscription> _logger;

    public AddOvasSubscription(IAddOvasSubscription ovasSubscription, ILoggerFactory loggerFactory)
    {
        _ovasSubscription = ovasSubscription;
        _logger = loggerFactory.CreateLogger<AddOvasSubscription>();
    }

    [Function("AddOvasSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "ovas/subscriptions")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, ShortSeriesSubscriptionContext.Default.ShortSeriesSubscription);
        ArgumentNullException.ThrowIfNull(payload);
        return await req
            .CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(param => _ovasSubscription.Subscribe(param.UserId, param.Series, param.NotificationTime, default))
            .ToResponse(req, _logger);
    }
}