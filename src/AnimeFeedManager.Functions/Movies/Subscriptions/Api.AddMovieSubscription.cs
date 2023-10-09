using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using ShortSeriesSubscriptionContext = AnimeFeedManager.Common.Dto.ShortSeriesSubscriptionContext;

namespace AnimeFeedManager.Functions.Movies.Subscriptions;

public class AddMovieSubscription
{
    private readonly IAddMovieSubscription _movieSubscription;
    private readonly ILogger<AddMovieSubscription> _logger;

    public AddMovieSubscription(IAddMovieSubscription movieSubscription, ILoggerFactory loggerFactory)
    {
        _movieSubscription = movieSubscription;
        _logger = loggerFactory.CreateLogger<AddMovieSubscription>();
    }

    [Function("AddMovieSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "movies/subscriptions")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, ShortSeriesSubscriptionContext.Default.ShortSeriesSubscription);
        ArgumentNullException.ThrowIfNull(payload);
        return await req
            .CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(param => _movieSubscription.Subscribe(param.UserId, param.Series, param.NotificationTime, default))
            .ToResponse(req, _logger);
    }
}