namespace AnimeFeedManager.Application.Mappers;

internal static class CommonMappers
{
    internal static DateTime? MapDate(Option<DateTime> date)
    {
        var unpacked = date.Match(
            a => a,
            () => DateTime.MinValue
        );
        return unpacked != DateTime.MinValue ? unpacked.ToUniversalTime() : null;
    }
}