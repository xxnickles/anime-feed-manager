using AnimeFeedManager.Features.Common;

namespace AnimeFeedManager.Features.Tests.Helpers;

internal static class TestSeasons
{
    /// <summary>A valid current-year season for tests that need one but do not assert on its value.</summary>
    internal static SeriesSeason Default => new(Season.Spring(), Year.FromNumber(DateTime.Now.Year));
}
