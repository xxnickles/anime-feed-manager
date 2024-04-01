using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Web.Features.Ovas.Controls;

public class OvaControlData
{
    public string Title { get; set; } = string.Empty;
    public DateTime NotificationTime { get; set; } = default;
    public string UserId { get; set; } = string.Empty;
 
    
    public static implicit operator OvaControlData(UnsubscribedOva ova)
    {
        return new OvaControlData
        {
            Title = ova.Title,
            UserId = ova.UserId,
            NotificationTime = ova.AirDate
        };
    }
    
    public static implicit operator OvaControlData(SubscribedOva ova)
    {
        return new OvaControlData
        {
            Title = ova.Title,
            UserId = ova.UserId,
            NotificationTime = ova.AirDate
        };
    }
}