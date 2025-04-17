using AnimeFeedManager.Old.Common.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Features.Seasons.Types;

public sealed class SeasonExistError : DomainError
{
    private SeasonExistError(string message) : base(message)
    {
    }

    public static SeasonExistError Create(SeasonStorage season) =>
        new($"'{season.Year}-{season.Season}' already exist");

    public override void LogError(ILogger logger)
    {
        logger.LogWarning("{Error}", Message);
    }
}