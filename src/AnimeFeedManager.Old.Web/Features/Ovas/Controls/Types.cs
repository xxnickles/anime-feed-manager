using AnimeFeedManager.Old.Common.Dto;

namespace AnimeFeedManager.Old.Web.Features.Ovas.Controls;

public class OvaControlData
{
    public string Title { get; set; } = string.Empty;
    public DateTime NotificationTime { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LoaderSelector { get; set; } = string.Empty;
    
    public bool HasFeed { get; set; }

    public static OvaControlData MapFrom(UnsubscribedOva ova, string loaderSelector)
    {
        return new OvaControlData
        {
            Title = ova.Title,
            UserId = ova.UserId,
            NotificationTime = ova.AirDate,
            LoaderSelector = loaderSelector,
            HasFeed = ova.Links.Length > 0
        };
    }

    public static OvaControlData MapFrom(SubscribedOva ova, string loaderSelector)
    {
        return new OvaControlData
        {
            Title = ova.Title,
            UserId = ova.UserId,
            NotificationTime = ova.AirDate,
            LoaderSelector = loaderSelector,
            HasFeed = ova.Links.Length > 0
        };
    }
}

public record AdminOvaControlParams(string Id, string Title, string Season, bool HasFeed)
{
    public static implicit operator AdminOvaControlParams(OvaForUser animeForUser) => animeForUser switch
    {
        { IsAdmin: true } => new AdminOvaControlParams(animeForUser.Id, animeForUser.Title, animeForUser.Season,
            animeForUser.Links.Length > 0),
        _ => new DefaultAdminOvaControlParams()
    };
}

public record DefaultAdminOvaControlParams() : AdminOvaControlParams(string.Empty, string.Empty, string.Empty, false);