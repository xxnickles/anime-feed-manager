namespace AnimeFeedManager.WebApp.Services;

public interface IAdminService
{ 
    Task UpdateTvLibrary();
    Task UpdateTvTitles();

    Task UpdateOvasLibrary(string season, ushort year);
    Task UpdateMoviesLibrary(string season, ushort year);
}

public class AdminService(HttpClient httpClient) : IAdminService
{
    public Task UpdateTvLibrary()
    {
        return httpClient.PostAsync("/api/tv/library", null);
    }

    public Task UpdateTvTitles()
    {
        return httpClient.PostAsync("/api/tv/titles", null);
    }

    public async Task UpdateOvasLibrary(string season, ushort year)
    {
        var response =await httpClient.PostAsync($"/api/ovas/library/{year}/{season}", null);
        await response.CheckForProblemDetails();
    }
        
    public async Task UpdateMoviesLibrary(string season, ushort year)
    {
        var response =await httpClient.PostAsync($"/api/movies/library/{year}/{season}", null);
        await response.CheckForProblemDetails();
    }

}