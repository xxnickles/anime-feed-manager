using System.Globalization;

namespace AnimeFeedManager.Web.Features.Tv.SeriesReleases;

internal static class ReleaseTimes
{
    internal static string Format(DateTimeOffset at) =>
        at.UtcDateTime.ToString("ddd, MMM d · HH:mm", CultureInfo.InvariantCulture);

    internal static string Format(DateTime utc) =>
        utc.ToString("ddd, MMM d · HH:mm", CultureInfo.InvariantCulture);

    internal static string Date(DateTimeOffset at) =>
        at.UtcDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture);
}
