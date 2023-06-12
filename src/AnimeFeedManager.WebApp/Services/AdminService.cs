namespace AnimeFeedManager.WebApp.Services;

public interface IAdminService
{ 
    Task UpdateTvLibrary();
    Task UpdateTvTitles();

    Task UpdateOvasLibrary(string season, ushort year);
    Task UpdateMoviesLibrary(string season, ushort year);
    Task SetAllSeriesAsNoCompleted();
}

public class AdminService : IAdminService
{
    private readonly HttpClient _httpClient;

    public AdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
        
    public Task UpdateTvLibrary()
    {
        return _httpClient.PostAsync("/api/tv/library", null);
    }

    public Task UpdateTvTitles()
    {
        return _httpClient.PostAsync("/api/tv/titles", null);
    }

    public async Task UpdateOvasLibrary(string season, ushort year)
    {
        var response =await _httpClient.PostAsync($"/api/ovas/{year}/{season}", null);
        await response.CheckForProblemDetails();
    }
        
    public async Task UpdateMoviesLibrary(string season, ushort year)
    {
        var response =await _httpClient.PostAsync($"/api/movies/{year}/{season}", null);
        await response.CheckForProblemDetails();
    }

    public Task SetAllSeriesAsNoCompleted()
    {
        return _httpClient.PostAsync("/api/management/set-status", null);
    }

}