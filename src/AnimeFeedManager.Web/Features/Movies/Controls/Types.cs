using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Web.Features.Movies.Controls;

public class MovieControlData
{
    public string Title { get; set; } = string.Empty;
    public DateTime NotificationTime { get; set; } = default;
    public string UserId { get; set; } = string.Empty;
 
    
    public static implicit operator MovieControlData(UnsubscribedMovie movie)
    {
        return new MovieControlData
        {
            Title = movie.Title,
            UserId = movie.UserId,
            NotificationTime = movie.AirDate
        };
    }
    
    public static implicit operator MovieControlData(SubscribedMovie movie)
    {
        return new MovieControlData
        {
            Title = movie.Title,
            UserId = movie.UserId,
            NotificationTime = movie.AirDate
        };
    }
}