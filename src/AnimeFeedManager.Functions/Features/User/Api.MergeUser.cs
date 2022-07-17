using AnimeFeedManager.Application.User.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace User
{
    public class MergeUser
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public MergeUser(IMediator mediator, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger<MergeUser>();
        }

        [Function("MergeUser")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "user")] HttpRequestData req)
        {
            var dto = await Serializer.FromJson<UserDto>(req.Body);
            ArgumentNullException.ThrowIfNull(dto);
            var command = new MergeUserCmd(dto.UserId, dto.Email);
            return await _mediator.Send(command).ToResponse(req, _logger);
        }
    }
}