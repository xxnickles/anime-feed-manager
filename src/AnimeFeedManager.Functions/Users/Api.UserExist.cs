﻿using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Users.IO;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Users;

public class UserExist(IUserVerification userGetter, ILoggerFactory loggerFactory)
{
    private readonly ILogger<UserExist> _logger = loggerFactory.CreateLogger<UserExist>();

    [Function("UserExist")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{id}")]
        HttpRequestData req,
        string id, CancellationToken token)
    {
        return await UserId.Validate(id)
            .ValidationToEither()
            .BindAsync(userId => userGetter.UserExist(userId, token))
            .ToResponse(req, _logger);
    }
}