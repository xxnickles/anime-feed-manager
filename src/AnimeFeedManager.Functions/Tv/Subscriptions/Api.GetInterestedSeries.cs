using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public class GetInterestedSeries(IGetInterestedSeries getInterestedSeries, ILoggerFactory loggerFactory)
{
    private readonly ILogger<GetInterestedSeries> _logger = loggerFactory.CreateLogger<GetInterestedSeries>();

    [Function("GetTvInterestedSeries")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tv/interested/{subscriber}")]
        HttpRequestData req,
        string subscriber,
        CancellationToken token
    )
    {
        return await req.CheckAuthorization()
            .BindAsync(_ => UserId.Parse(subscriber))
            .BindAsync(user => getInterestedSeries.Get(user, token))
            .MapAsync(items => items.ConvertAll(i => i.RowKey))
            .ToResponse(req, _logger);
    }
}