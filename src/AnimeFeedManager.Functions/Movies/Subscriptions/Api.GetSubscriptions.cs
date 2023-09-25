using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Subscriptions;

public class GetSubscriptions
{
    private readonly IGetMovieSubscriptions _movieSubscriptions;
    private readonly ILogger<GetSubscriptions> _logger;

    public GetSubscriptions(IGetMovieSubscriptions movieSubscriptions, ILoggerFactory loggerFactory)
    {
        _movieSubscriptions = movieSubscriptions;
        _logger = loggerFactory.CreateLogger<GetSubscriptions>();
    }

    [Function("GetMoviesSubscriptions")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "movies/subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await UserIdValidator.Validate(subscriber).ValidationToEither()
            .BindAsync(id => _movieSubscriptions.GetSubscriptions(id, default))
            .ToResponse(req, _logger);
    }
}