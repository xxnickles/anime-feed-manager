using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Ovas;

public class UpdateOvasLibraryOutput
{
    [QueueOutput(QueueNames.OvasLibraryUpdate)]
    public OvasUpdate? UpdatePayload { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class UpdateOvasLibrary
{
    private readonly ILogger _logger;

    public UpdateOvasLibrary(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<UpdateOvasLibrary>();
    }


    [Function("UpdateLatestOvasLibrary")]
    public async Task<UpdateOvasLibraryOutput> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ovas/library")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of latest ova library (Manual trigger)");

        var result = await req.AllowAdminOnly();

        return await result.Match(
            _ => OkResponse(req, new OvasUpdate(ShortSeriesUpdateType.Latest, new NullSeasonInfo())),
            e => ErrorResponse(req, e)
        );
    }

    [Function("UpdateSeasonOvasLibrary")]
    public async Task<UpdateOvasLibraryOutput> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ovas/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        _logger.LogInformation("Automated Update of latest ova library (Manual trigger)");

        var result = await req.AllowAdminOnly();

        return await result.Match(
            _ => OkResponse(req, new OvasUpdate(ShortSeriesUpdateType.Season, new SeasonInfoDto(season, year))),
            e => ErrorResponse(req, e)
        );
    }

    private static async Task<UpdateOvasLibraryOutput> OkResponse(HttpRequestData req, OvasUpdate updatePayload)
    {
        return new UpdateOvasLibraryOutput
        {
            UpdatePayload = updatePayload,
            HttpResponse = await req.Ok()
        };
    }

    private async Task<UpdateOvasLibraryOutput> ErrorResponse(HttpRequestData req, DomainError error)
    {
        return new UpdateOvasLibraryOutput
        {
            UpdatePayload = null,
            HttpResponse = await error.ToResponse(req, _logger)
        };
    }
}