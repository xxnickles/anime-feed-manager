using System.ComponentModel.DataAnnotations;
using AnimeFeedManager.Features.Tv.FeedManager.Search;

namespace AnimeFeedManager.Web.Features.Admin.FeedManager;

public abstract record SearchOutcome;

public sealed record NotSearched : SearchOutcome;

public sealed record Searched(ReleaseSearchResults Results) : SearchOutcome;

public sealed record SearchFailed(string Message) : SearchOutcome;

public class FeedSeriesViewModel
{
    [Required(AllowEmptyStrings = false)] public string SeriesId { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)] public string SeriesTitle { get; set; } = string.Empty;
}

public class EpisodesOverrideViewModel : FeedSeriesViewModel
{
    [Range(1, 9999, ErrorMessage = "Episodes must be a positive count")]
    public int Episodes { get; set; }
}

public class BindReleaseViewModel
{
    [Required(AllowEmptyStrings = false)] public string SeriesId { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)] public string SeriesTitle { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)] public string Group { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)] public string ReleaseTitle { get; set; } = string.Empty;

    // A release without tags has an empty variant.
    public string? Variant { get; set; }
    public bool Trusted { get; set; }
}
