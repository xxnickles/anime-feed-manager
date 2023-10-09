using System.Net;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;
using SimpleUserContext = AnimeFeedManager.Common.Dto.SimpleUserContext;

namespace AnimeFeedManager.WebApp.Services
{
    public interface IUserService
    {
        public Task MergeUser(SimpleUser user, CancellationToken cancellationToken = default);
        public Task<bool> UserExist(string id, CancellationToken cancellationToken = default);
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

        public async Task<bool> UserExist(string id, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"api/user/{id}", cancellationToken);
            return response.StatusCode == HttpStatusCode.Accepted;
        }
    }
}