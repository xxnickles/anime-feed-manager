using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Web.Features.Tv.Controls;

public class AvailableTvSeriesControlData
{
    public string Title { get; set; } = string.Empty;
    public string FeedId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    
    
    public static implicit operator AvailableTvSeriesControlData(SubscribedAnime anime)
    {
        return new AvailableTvSeriesControlData
        {
            Title = anime.Title,
            UserId = anime.UserId.ToString(),
            FeedId = anime.FeedId
        };
    }
    
    public static implicit operator AvailableTvSeriesControlData(UnSubscribedAnime anime)
    {
        return new AvailableTvSeriesControlData
        {
            Title = anime.Title,
            UserId = anime.UserId.ToString(),
            FeedId = anime.FeedId
        };
    }
}

public class NotAvailableControlData
{
    public string Title { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public static implicit operator NotAvailableControlData(InterestedAnime anime)
    {
        return new NotAvailableControlData
        {
            Title = anime.AnimeTitle,
            UserId = anime.UserId.ToString()
        };
    }

    public static implicit operator NotAvailableControlData(NotAvailableAnime anime)
    {
        return new NotAvailableControlData
        {
            Title = anime.AnimeTitle,
            UserId = anime.UserId.ToString()
        };
    }
}