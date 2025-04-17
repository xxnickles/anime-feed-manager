using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Dto;

namespace AnimeFeedManager.Web.Features.Tv.Controls;

public class AvailableTvSeriesControlData
{
    public string Title { get; set; } = string.Empty;
    public string FeedId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string LoaderSelector { get; set; } = string.Empty;

    public static AvailableTvSeriesControlData MapFrom(SubscribedAnime anime, string loaderSelector)
    {
        return new AvailableTvSeriesControlData
        {
            Title = anime.Title,
            UserId = anime.UserId,
            FeedId = anime.FeedId,
            LoaderSelector = loaderSelector
        };
    }

    public static AvailableTvSeriesControlData MapFrom(UnSubscribedAnime anime, string loaderSelector)
    {
        return new AvailableTvSeriesControlData
        {
            Title = anime.Title,
            UserId = anime.UserId,
            FeedId = anime.FeedId,
            LoaderSelector = loaderSelector
        };
    }
}

public class NotAvailableControlData
{
    public string Title { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string SeriesId { get; set; } = string.Empty;

    public string LoaderSelector { get; set; } = string.Empty;

    public static NotAvailableControlData MapFrom(InterestedAnime anime, string loaderSelector)
    {
        return new NotAvailableControlData
        {
            Title = anime.Title,
            UserId = anime.UserId,
            SeriesId = anime.Id,
            LoaderSelector = loaderSelector
        };
    }

    public static NotAvailableControlData MapFrom(NotAvailableAnime anime, string loaderSelector)
    {
        return new NotAvailableControlData
        {
            Title = anime.Title,
            UserId = anime.UserId,
            SeriesId = anime.Id,
            LoaderSelector = loaderSelector
        };
    }
}

public record AdminTvControlParams(string Id, string Title, string Season, SeriesStatus Status)
{
    public static implicit operator AdminTvControlParams(AnimeForUser animeForUser) => animeForUser switch
    {
        {IsAdmin: true} => new AdminTvControlParams(animeForUser.Id, animeForUser.Title, animeForUser.Season,
            animeForUser.SeriesStatus),
        _ => new DefaultAdminTvControlParams()
    };
}

public record DefaultAdminTvControlParams()
    : AdminTvControlParams(string.Empty, string.Empty, string.Empty, SeriesStatus.NotAvailable);

public record AlternativeTitleUpdate(string Id, string Season, string Title, string OriginalTitle, string Status);