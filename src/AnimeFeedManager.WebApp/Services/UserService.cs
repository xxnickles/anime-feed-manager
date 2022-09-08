using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface IUserService
{
    public Task MergeUser(UserDto user, CancellationToken cancellationToken = default);
    public Task<string?> GetEmail(string id, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public Task MergeUser(UserDto user,CancellationToken cancellationToken = default)
    {
        return _httpClient.PostAsJsonAsync("api/user", user, cancellationToken: cancellationToken);
    }

    public async Task<string?> GetEmail(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/user/{id}", cancellationToken);
        return await response.MapToObject<string?>(null);
    }
}