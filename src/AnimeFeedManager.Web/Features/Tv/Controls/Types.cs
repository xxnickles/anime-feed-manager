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

public record AdminTvControlParams(string Id, string Title, string Season)
{
    public static implicit operator AdminTvControlParams(AnimeForUser animeForUser) => animeForUser switch
    {
        {IsAdmin: true} => new AdminTvControlParams(animeForUser.Id, animeForUser.Title, animeForUser.Season),
        _ => new DefaultAdminTvControlParams()
    };
}

public record DefaultAdminTvControlParams() : AdminTvControlParams(string.Empty, string.Empty, string.Empty);

public record AlternativeTitleUpdate(string Id, string Season, string Title);
public record SeriesToRemove(string Id, string Season);