using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Users.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Users;

public class UserExist
{
    private readonly IUserGetter _userGetter;

    private readonly ILogger<UserExist> _logger;

    public UserExist(IUserGetter userGetter, ILoggerFactory loggerFactory)
    {
        _userGetter = userGetter;
        _logger = loggerFactory.CreateLogger<UserExist>();
    }

    [Function("UserExist")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{id}")]
        HttpRequestData req,
        string id)
    {
        return await UserIdValidator.Validate(id)
            .ValidationToEither()
            .BindAsync(userId => _userGetter.UserExist(userId, default))
            .ToResponse(req, _logger);
    }
}