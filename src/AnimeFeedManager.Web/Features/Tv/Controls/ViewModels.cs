using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Web.Features.Tv.Controls;

public class TvSeriesCardViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string SeriesId { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string SeriesTitle { get; set; } = string.Empty;
    
   
}

// Subscribe/interested forms. Feed data comes from storage; LoaderSelector is render-only (not posted).
public class TvSeriesActionViewModel : TvSeriesCardViewModel
{
    public string LoaderSelector { get; set; } = string.Empty;
}

public class AlternativeTitlesViewModel : TvSeriesCardViewModel
{
    public string[]? AlternativeTitles { get; set; }
}

public class RemoveSeriesViewModel : TvSeriesCardViewModel
{
    public string LoaderSelector { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string CardId { get; set; } = string.Empty;
}

