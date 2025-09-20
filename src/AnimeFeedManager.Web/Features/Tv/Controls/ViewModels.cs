using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Web.Features.Tv.Controls;

public class TvInterestedViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string UserId { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string SeriesId { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string SeriesTitle { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string LoaderSelector { get; set; } = string.Empty;
    public bool AdminView { get; set; }
}

public class TvSubscriptionViewModel : TvInterestedViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string SeriesFeedTitle { get; set; } = string.Empty;
}

