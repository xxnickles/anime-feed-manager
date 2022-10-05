namespace AnimeFeedManager.WebApp.Services
{
    public interface IAdminService
    {
        Task UpdateTvLibrary(CancellationToken cancellationToken = default);
        Task UpdateTvTitles(CancellationToken cancellationToken = default);
        Task SetAllSeriesAsNoCompleted(CancellationToken cancellationToken = default);
    }

    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task UpdateTvLibrary(CancellationToken cancellationToken = default)
        {
            return _httpClient.PostAsync("/api/tv/library", null, cancellationToken);
        }

        public Task UpdateTvTitles(CancellationToken cancellationToken = default)
        {
            return _httpClient.PostAsync("/api/tv/titles", null, cancellationToken);
        }

        public Task SetAllSeriesAsNoCompleted(CancellationToken cancellationToken = default)
        {
            return _httpClient.PostAsync("/api/management/set-status", null, cancellationToken);
        }

    }
}
