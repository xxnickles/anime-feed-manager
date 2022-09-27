using AnimeFeedManager.Application.User.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.User
{
    public class GetUserEmail
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public GetUserEmail(IMediator mediator, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger<GetUserEmail>();
        }

        [Function("GetUserEmail")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{id}")]
            HttpRequestData req,
            string id)
        {
            return await _mediator.Send(new GetUserEmailQry(id)).ToResponse(req, _logger);
        }
    }
}