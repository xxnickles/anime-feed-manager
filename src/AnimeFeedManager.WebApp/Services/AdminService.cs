namespace AnimeFeedManager.WebApp.Services
{
    public interface IAdminService
    {
        Task UpdateLibrary(CancellationToken cancellationToken = default);
        Task SetAllSeriesAsNoCompleted(CancellationToken cancellationToken = default);
    }

    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task UpdateLibrary(CancellationToken cancellationToken = default)
        {
            return _httpClient.PostAsync("/api/scrapping/library", null, cancellationToken);
        }

        public Task SetAllSeriesAsNoCompleted(CancellationToken cancellationToken = default)
        {
            return _httpClient.PostAsync("/api/management/set-status", null, cancellationToken);
        }

    }
}
