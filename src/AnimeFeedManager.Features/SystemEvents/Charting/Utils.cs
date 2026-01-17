namespace AnimeFeedManager.Features.SystemEvents.Charting;

internal static class Utils
{
    internal static IEnumerable<DateTime> GenerateDateRange(DateTime from, DateTime to)
    {
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            yield return date;
        }
    }
}