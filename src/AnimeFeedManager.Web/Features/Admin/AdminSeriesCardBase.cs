using AnimeFeedManager.Old.Common.Domain;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Admin;

public abstract class AdminSeriesCardBase : ComponentBase
{
    internal readonly string LatestSeasonLoader = IdHelpers.GetUniqueName("admin-card-loader");
    internal readonly string BySeasonLoader = IdHelpers.GetUniqueName("admin-card-loader");
    
    internal readonly int MaxYear = DateTimeOffset.UtcNow.Year + 1;
    internal const int MinYear = 2000;
    internal int Year { get; set; } = DateTimeOffset.UtcNow.Year;
    internal string SelectedSeason { get; set; } = Season.Spring.ToString();

}