using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using ShortSeriesUnsubscribeContext = AnimeFeedManager.Common.Dto.ShortSeriesUnsubscribeContext;

namespace AnimeFeedManager.Functions.Ovas.Subscriptions;

public class RemoveOvasSubscription(IRemoveOvasSubscription ovasSubscription, ILoggerFactory loggerFactory)
{
    private readonly ILogger<RemoveOvasSubscription> _logger = loggerFactory.CreateLogger<RemoveOvasSubscription>();

    [Function("RemoveOvasSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "ovas/subscriptions/unsubscribe")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, ShortSeriesUnsubscribeContext.Default.ShortSeriesUnsubscribe);
        ArgumentNullException.ThrowIfNull(payload);
        return await req
            .CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(param => ovasSubscription.Unsubscribe(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}