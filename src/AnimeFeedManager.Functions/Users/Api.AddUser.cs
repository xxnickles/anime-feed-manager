using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Users.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using SimpleUserContext = AnimeFeedManager.Common.Dto.SimpleUserContext;

namespace AnimeFeedManager.Functions.Users;

public sealed class AddUser(IUserStore userStore, ILoggerFactory loggerFactory)
{
    private readonly ILogger<AddUser> _logger = loggerFactory.CreateLogger<AddUser>();

    [Function("AddUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "user")]
        HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync(req.Body, SimpleUserContext.Default.SimpleUser);
        ArgumentNullException.ThrowIfNull(payload);
        return await Validate(payload)
            .BindAsync(param => userStore.AddUser(param.UserId, param.Email, default))
            .ToResponse(req, _logger);
    }


    private Either<DomainError, (Email Email, UserId UserId)> Validate(Common.Dto.SimpleUser payload)
    {
        return (EmailValidator.Validate(payload.Email), UserIdValidator.Validate(payload.UserId))
            .Apply((email, userid) => (email, userid))
            .ValidationToEither();
    }
}