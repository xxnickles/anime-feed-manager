namespace AnimeFeedManager.Features.Scrapping.SubsPlease;

/// <summary>Weekday name → shows airing that day.</summary>
internal sealed record SubsPleaseScheduleResponse(Dictionary<string, SubsPleaseScheduleEntry[]> Schedule);

internal sealed record SubsPleaseScheduleEntry(string Title);
