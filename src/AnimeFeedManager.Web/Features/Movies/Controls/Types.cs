using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Web.Features.Movies.Controls;

public class MovieControlData
{
    public string Title { get; set; } = string.Empty;
    public DateTime NotificationTime { get; set; } = default;
    public string UserId { get; set; } = string.Empty;
    public string LoaderSelector { get; set; } = string.Empty;
    
    public static MovieControlData MapFrom(UnsubscribedMovie movie, string loaderSelector)
    {
        return new MovieControlData
        {
            Title = movie.Title,
            UserId = movie.UserId,
            NotificationTime = movie.AirDate,
            LoaderSelector = loaderSelector
        };
    }
    
    public static MovieControlData MapFrom(SubscribedMovie movie, string loaderSelector)
    {
        return new MovieControlData
        {
            Title = movie.Title,
            UserId = movie.UserId,
            NotificationTime = movie.AirDate,
            LoaderSelector = loaderSelector
        };
    }
}

public record AdminMovieControlParams(string Id, string Title, string Season)
{
    public static implicit operator AdminMovieControlParams(MovieForUser animeForUser) => animeForUser switch
    {
        {IsAdmin: true} => new AdminMovieControlParams(animeForUser.Id, animeForUser.Title, animeForUser.Season),
        _ => new DefaultAdminMovieControlParams()
    };
}

public record DefaultAdminMovieControlParams() : AdminMovieControlParams(string.Empty, string.Empty, string.Empty);