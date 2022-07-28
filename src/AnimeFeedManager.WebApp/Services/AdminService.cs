namespace AnimeFeedManager.WebApp.Services
{
    public interface IAdminService
    {
        Task UpdateLatestSeason();
        Task SetAllSeriesAsNoCompleted();
    }

    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task UpdateLatestSeason()
        {
            return _httpClient.PostAsync("/api/library", null);
        }

        public Task SetAllSeriesAsNoCompleted()
        {
            return _httpClient.PostAsync("/api/management/set-status", null);
        }

    }
}
