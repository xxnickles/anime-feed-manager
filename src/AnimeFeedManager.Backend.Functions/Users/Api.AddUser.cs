using System.Text.Json;
using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Users.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Users;

public sealed class AddUser
{
    private readonly IUserStore _userStore;
    private readonly ILogger<AddUser> _logger;

    public AddUser(IUserStore userStore, ILoggerFactory loggerFactory)
    {
        _userStore = userStore;
        _logger = loggerFactory.CreateLogger<AddUser>();
    }

    [Function("AddUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "user")]
        HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync(req.Body, SimpleUserContext.Default.SimpleUser);
        ArgumentNullException.ThrowIfNull(payload);
        return await Validate(payload)
            .BindAsync(param => _userStore.AddUser(param.UserId, param.Email, default))
            .ToResponse(req, _logger);
    }


    private Either<DomainError, (Email Email, UserId UserId)> Validate(SimpleUser payload)
    {
       return (EmailValidator.Validate(payload.Email), UserIdValidator.Validate(payload.UserId))
            .Apply((email, userid) => (email, userid))
            .ValidationToEither();
    }
}