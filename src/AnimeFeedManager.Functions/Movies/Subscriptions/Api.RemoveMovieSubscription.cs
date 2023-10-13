using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using ShortSeriesUnsubscribeContext = AnimeFeedManager.Common.Dto.ShortSeriesUnsubscribeContext;

namespace AnimeFeedManager.Functions.Movies.Subscriptions;

public class RemoveMovieSubscription
{
    private readonly IRemoveMovieSubscription _movieSubscription;
    private readonly ILogger<RemoveMovieSubscription> _logger;

    public RemoveMovieSubscription(IRemoveMovieSubscription movieSubscription, ILoggerFactory loggerFactory)
    {
        _movieSubscription = movieSubscription;
        _logger = loggerFactory.CreateLogger<RemoveMovieSubscription>();
    }

    [Function("RemoveMovieSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "movies/subscriptions/unsubscribe")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, ShortSeriesUnsubscribeContext.Default.ShortSeriesUnsubscribe);
        ArgumentNullException.ThrowIfNull(payload);
        return await req
            .CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(param => _movieSubscription.Unsubscribe(param.UserId, param.Series, default))
            .ToResponse(req, _logger);
    }
}