using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using ShortSeriesUnsubscribeContext = AnimeFeedManager.Common.Dto.ShortSeriesUnsubscribeContext;

namespace AnimeFeedManager.Functions.Movies.Subscriptions;

public class RemoveMovieSubscription(IRemoveMovieSubscription movieSubscription, ILoggerFactory loggerFactory)
{
    private readonly ILogger<RemoveMovieSubscription> _logger = loggerFactory.CreateLogger<RemoveMovieSubscription>();

    [Function("RemoveMovieSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "movies/subscriptions/unsubscribe")]
        HttpRequestData req, CancellationToken token)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, ShortSeriesUnsubscribeContext.Default.ShortSeriesUnsubscribe, token);
        ArgumentNullException.ThrowIfNull(payload);
        return await req
            .CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(param => movieSubscription.Unsubscribe(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}