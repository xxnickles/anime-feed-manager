using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface IUserService
{
    public Task MergeUser(UserDto user);
    public Task<string> GetEmail(string id);
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
        return _httpClient.PostAsJsonAsync("user", user);
    }

    public Task<string> GetEmail(string id)
    {
        return _httpClient.GetStringAsync($"user/{id}");
    }
}