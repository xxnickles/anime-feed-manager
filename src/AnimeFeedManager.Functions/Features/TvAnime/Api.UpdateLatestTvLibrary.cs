using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime
{
    public class UpdateLatestTvLibraryOutput
    {
        [QueueOutput(QueueNames.TvAnimeLibraryUpdate)] 
        public LibraryUpdate? StartLibraryUpdate { get; set; }

        public HttpResponseData? HttpResponse { get; set; }
    }

    public class UpdateLatestTvLibrary
    {
        private readonly ILogger _logger;

        public UpdateLatestTvLibrary(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateLatestTvLibrary>();
        }

        [Function("UpdateLatestTvLibrary")]
        public async Task<UpdateLatestTvLibraryOutput> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/library")] HttpRequestData req)
        {
            _logger.LogInformation("Automated Update of Library (Manual trigger)");

            var result = await req.AllowAdminOnly();

            return await result.Match(
                _ => OkResponse(req),
                e => ErrorResponse(req, e)
            );
        }

        private static async Task<UpdateLatestTvLibraryOutput> OkResponse(HttpRequestData req)
        {
            return new UpdateLatestTvLibraryOutput
            {
                StartLibraryUpdate = new LibraryUpdate(TvUpdateType.Full),
                HttpResponse = await req.Ok()
            };
        }

        private async Task<UpdateLatestTvLibraryOutput> ErrorResponse(HttpRequestData req, DomainError error)
        {

            return new UpdateLatestTvLibraryOutput
            {
                StartLibraryUpdate = null,
                HttpResponse = await error.ToResponse(req, _logger)
            };
        }
    }
}
