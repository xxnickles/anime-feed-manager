using System.Net;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;
using SimpleUserContext = AnimeFeedManager.Common.Dto.SimpleUserContext;

namespace AnimeFeedManager.WebApp.Services;

public interface IUserService
{
    public Task MergeUser(SimpleUser user, CancellationToken cancellationToken = default);
    public Task<bool> UserExist(string id, CancellationToken cancellationToken = default);
}

public class UserService(HttpClient httpClient) : IUserService
{
    public async Task MergeUser(SimpleUser user, CancellationToken cancellationToken = default)
    {
        var result = await httpClient.PostAsJsonAsync("api/user", user, SimpleUserContext.Default.SimpleUser,
            cancellationToken: cancellationToken);
        await result.CheckForProblemDetails();
    }

    public async Task<bool> UserExist(string id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/user/{id}", cancellationToken);
        return response.StatusCode == HttpStatusCode.Accepted;
    }
}