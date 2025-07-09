using AnimeFeedManager.Old.Common.Dto;

namespace AnimeFeedManager.Old.Web.Features.Movies.Controls;

public class MovieControlData
{
    public string Title { get; set; } = string.Empty;
    public DateTime NotificationTime { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LoaderSelector { get; set; } = string.Empty;
    public bool HasFeed { get; set; } 
    
    public static MovieControlData MapFrom(UnsubscribedMovie movie, string loaderSelector)
    {
        return new MovieControlData
        {
            Title = movie.Title,
            UserId = movie.UserId,
            NotificationTime = movie.AirDate,
            LoaderSelector = loaderSelector,
            HasFeed = movie.Links.Length > 0
        };
    }
    
    public static MovieControlData MapFrom(SubscribedMovie movie, string loaderSelector)
    {
        return new MovieControlData
        {
            Title = movie.Title,
            UserId = movie.UserId,
            NotificationTime = movie.AirDate,
            LoaderSelector = loaderSelector,
            HasFeed = movie.Links.Length > 0
        };
    }
}

public record AdminMovieControlParams(string Id, string Title, string Season, bool HasFeed)
{
    public static implicit operator AdminMovieControlParams(MovieForUser animeForUser) => animeForUser switch
    {
        {IsAdmin: true} => new AdminMovieControlParams(animeForUser.Id, animeForUser.Title, animeForUser.Season, animeForUser.Links.Length > 0),
        _ => new DefaultAdminMovieControlParams()
    };
}

public record DefaultAdminMovieControlParams() : AdminMovieControlParams(string.Empty, string.Empty, string.Empty, false);