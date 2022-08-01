namespace AnimeFeedManager.WebApp.Services
{
    public interface IAdminService
    {
        Task UpdateLibrary();
        Task SetAllSeriesAsNoCompleted();
    }

    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task UpdateLibrary()
        {
            return _httpClient.PostAsync("/api/scrapping/library", null);
        }

        public Task SetAllSeriesAsNoCompleted()
        {
            return _httpClient.PostAsync("/api/management/set-status", null);
        }

    }
}
