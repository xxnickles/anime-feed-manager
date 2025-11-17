using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Web.Features.Tv.Controls;

public class TvSeriesCardViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string SeriesId { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string SeriesTitle { get; set; } = string.Empty;
    
   
}

public class TvInterestedViewModel : TvSeriesCardViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string LoaderSelector { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string CardBadgeId { get; set; } = string.Empty;

}

public class TvSubscriptionViewModel : TvInterestedViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string SeriesFeedTitle { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string SeriesLink { get; set; } = string.Empty;
}

public class AlternativeTitlesViewModel : TvSeriesCardViewModel
{
    public string[]? AlternativeTitles { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string Season { get; set; } = string.Empty;
}

public class RemoveSeriesViewModel : TvSeriesCardViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string Season { get; set; } = string.Empty;
    
   
    public string LoaderSelector { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string CardId { get; set; } = string.Empty;
}

