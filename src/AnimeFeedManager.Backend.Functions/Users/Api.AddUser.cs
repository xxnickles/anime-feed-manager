using System.Text.Json;
using AnimeFeedManager.Features.Common.Dto;
using AnimeFeedManager.Features.Common.Types;
using AnimeFeedManager.Features.Users.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Users;

public class AddUser
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
        return await EmailValidators.ValidateEmail(payload.Email)
            .BindAsync(email => _userStore.AddUser(payload.UserId, email, default))
            .ToResponse(req, _logger);
    }
}