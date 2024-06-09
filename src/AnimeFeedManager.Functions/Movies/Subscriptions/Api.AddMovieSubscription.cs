using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using ShortSeriesSubscriptionContext = AnimeFeedManager.Common.Dto.ShortSeriesSubscriptionContext;

namespace AnimeFeedManager.Functions.Movies.Subscriptions;

public class AddMovieSubscription(IAddMovieSubscription movieSubscription, ILoggerFactory loggerFactory)
{
    private readonly ILogger<AddMovieSubscription> _logger = loggerFactory.CreateLogger<AddMovieSubscription>();

    [Function("AddMovieSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "movies/subscriptions")]
        HttpRequestData req, CancellationToken token)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body,
                ShortSeriesSubscriptionContext.Default.ShortSeriesSubscription, token);
        ArgumentNullException.ThrowIfNull(payload);
        return await req
            .CheckAuthorization()
            .BindAsync(_ => Utils.Validate(payload))
            .BindAsync(
                param => movieSubscription.Subscribe(param.UserId, param.Series, param.NotificationTime, token))
            .ToResponse(req, _logger);
    }
}