using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime;

public class UpdateFeedTitlesOutput
{
    [QueueOutput(QueueNames.TvAnimeLibraryUpdate)] 
    public LibraryUpdate? StartLibraryUpdate { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class UpdateFeedTitles
{
    private readonly ILogger _logger;

    public UpdateFeedTitles(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<UpdateLatestTvLibrary>();
    }

    [Function("UpdateFeedTitles")]
    public async Task<UpdateFeedTitlesOutput> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/titles")] HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Titles (Manual trigger)");

        var result = await req.AllowAdminOnly();

        return await result.Match(
            v => OkResponse(req),
            e => ErrorResponse(req, e)
        );
    }

    private static async Task<UpdateFeedTitlesOutput> OkResponse(HttpRequestData req)
    {
        return new UpdateFeedTitlesOutput
        {
            StartLibraryUpdate = new LibraryUpdate(TvUpdateType.Titles),
            HttpResponse = await req.Ok()
        };
    }

    private async Task<UpdateFeedTitlesOutput> ErrorResponse(HttpRequestData req, DomainError error)
    {

        return new UpdateFeedTitlesOutput
        {
            StartLibraryUpdate = null,
            HttpResponse = await error.ToResponse(req, _logger)
        };
    }
}