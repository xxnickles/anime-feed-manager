using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Scrapping
{
    public class UpdateLatestLibraryOutput
    {
        [QueueOutput(QueueNames.LibraryUpdate)] 
        public bool? StartLibraryUpdate { get; set; }

        public HttpResponseData? HttpResponse { get; set; }
    }

    public class UpdateLatestLibrary
    {
        private readonly ILogger _logger;

        public UpdateLatestLibrary(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateLatestLibrary>();
        }

        [Function("UpdateLatestLibrary")]
        public async Task<UpdateLatestLibraryOutput> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "scrapping/library")] HttpRequestData req)
        {
            _logger.LogInformation("Automated Update of Library");

            var result = await req.AllowAdminOnly();

            return await result.Match(
                v => OkResponse(req),
                e => ErrorResponse(req, e)
            );
        }

        private static async Task<UpdateLatestLibraryOutput> OkResponse(HttpRequestData req)
        {
            return new UpdateLatestLibraryOutput
            {
                StartLibraryUpdate = true,
                HttpResponse = await req.Ok()
            };
        }

        private async Task<UpdateLatestLibraryOutput> ErrorResponse(HttpRequestData req, DomainError error)
        {

            return new UpdateLatestLibraryOutput
            {
                StartLibraryUpdate = null,
                HttpResponse = await error.ToResponse(req, _logger)
            };
        }
    }
}
