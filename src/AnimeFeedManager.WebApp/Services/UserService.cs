using System.Net.Http.Json;
using AnimeFeedManager.Features.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface IUserService
{
    public Task MergeUser(SimpleUser user, CancellationToken cancellationToken = default);
    public Task<string?> GetEmail(string id, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task MergeUser(SimpleUser user, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.PostAsJsonAsync("api/user", user, SimpleUserContext.Default.SimpleUser,
            cancellationToken: cancellationToken);
        await result.CheckForProblemDetails();
    }

    public async Task<string?> GetEmail(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/user/{id}", cancellationToken);
        return await response.MapToString();
    }
}