using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using MediatR;

namespace AnimeFeedManager.Functions.Features.Library
{
    public class UpdateLatestLibraryOutput
    {
        [QueueOutput(QueueNames.AnimeLibrary)] public IEnumerable<string>? AnimeMessages { get; set; }

        [QueueOutput(QueueNames.AvailableSeasons, Connection = "AzureWebJobsStorage")]
        public string? SeasonMessage { get; set; }

        public HttpResponseData? HttpResponse { get; set; }
    }

    public class UpdateLatestLibrary
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public UpdateLatestLibrary(IMediator mediator, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger<UpdateLatestLibrary>();
        }

        [Function("UpdateLibrary")]
        public async Task<UpdateLatestLibraryOutput> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "library")] HttpRequestData req)
        {
            _logger.LogInformation("Manual Update of Library");

            var result = await req.AllowAdminOnly(new GetExternalLibraryQry())
                .BindAsync(r => _mediator.Send(r));

            return await result.Match(
                v => OkResponse(req, v),
                e => ErrorResponse(req, e)
            );
        }

        private async Task<UpdateLatestLibraryOutput> OkResponse(HttpRequestData req, ImmutableList<AnimeInfoStorage> series)
        {
            var seasonInfo = ExtractSeasonInformation(series.First());
            return new UpdateLatestLibraryOutput
            {
                AnimeMessages = series.Select(Serializer.ToJson),
                SeasonMessage = Serializer.ToJson(seasonInfo),
                HttpResponse = await req.Ok()
            };
        }

        private async Task<UpdateLatestLibraryOutput> ErrorResponse(HttpRequestData req, DomainError error)
        {

            return new UpdateLatestLibraryOutput
            {
                AnimeMessages = null,
                SeasonMessage = null,
                HttpResponse = await error.ToResponse(req, _logger)
            };
        }

        private static SeasonInfoDto ExtractSeasonInformation(AnimeInfoStorage sample) => new(sample?.Season ?? string.Empty, sample?.Year ?? 0);
    }
}
