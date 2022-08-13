using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface IUserService
{
    public Task MergeUser(UserDto user);
    public Task<string?> GetEmail(string id);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public Task MergeUser(UserDto user)
    {
        return _httpClient.PostAsJsonAsync("api/user", user);
    }

    public async Task<string?> GetEmail(string id)
    {
        var response = await _httpClient.GetAsync($"api/user/{id}");
        return await response.MapToObject<string?>(null);
    }
}