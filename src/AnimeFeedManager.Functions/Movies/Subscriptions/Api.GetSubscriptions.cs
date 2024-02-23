using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Subscriptions;

public class GetSubscriptions(IGetMovieSubscriptions movieSubscriptions, ILoggerFactory loggerFactory)
{
    private readonly ILogger<GetSubscriptions> _logger = loggerFactory.CreateLogger<GetSubscriptions>();

    [Function("GetMoviesSubscriptions")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "movies/subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await UserId.Parse(subscriber)
            .BindAsync(id => movieSubscriptions.GetSubscriptions(id, default))
            .ToResponse(req, _logger);
    }
}