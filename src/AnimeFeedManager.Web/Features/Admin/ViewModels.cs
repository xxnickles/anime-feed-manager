using AnimeFeedManager.Web.Common.Validation;

namespace AnimeFeedManager.Web.Features.Admin;

public class BySeasonViewModel
{
     [AsSeason]
     public string Season { get; set; } = string.Empty;
     
     [AsYear]
     public int Year { get; set; } = 0;
     
     public static BySeasonViewModel Current => new() { Season = Shared.Types.Season.Current, Year = Shared.Types.Year.Current};
}

/// <summary>
/// Marker class just for the sake of satisfy the compiler. Used for empty forms
/// </summary>
public class Noop {}